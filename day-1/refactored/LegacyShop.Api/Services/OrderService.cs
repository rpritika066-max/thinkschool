using LegacyShop.Api.DTOs;
using LegacyShop.Api.Models;
using LegacyShop.Api.Repositories;

namespace LegacyShop.Api.Services;

public sealed class OrderService(
    IOrderRepository repository,
    ILogger<OrderService> logger) : IOrderService
{
    private const decimal GoldDiscount = 0.85m;
    private const decimal SilverDiscount = 0.92m;
    private const decimal Bulk10Discount = 0.90m;
    private const decimal Bulk5Discount = 0.95m;
    private const decimal TaxRate = 0.0825m;

    public async Task<CreateOrderResult> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await repository.GetCustomerAsync(
            request.CustomerId,
            cancellationToken);

        if (customer is null)
        {
            return Failure(404, "Customer not found.");
        }

        if (customer.Status == "BLOCKED")
        {
            return Failure(400, "Customer is blocked.");
        }

        decimal subtotal = 0m;
        var totalItemCount = 0;
        var orderLines = new List<OrderLine>();

        foreach (var item in request.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var product = await repository.GetProductAsync(
                item.ProductId,
                cancellationToken);

            if (product is null)
            {
                return Failure(404, $"Product {item.ProductId} not found.");
            }

            if (product.IsDiscontinued)
            {
               await repository.AddDiscontinuedHitAsync(
    new DiscontinuedHit
    {
        ProductId = product.Id,
        Hit = DateTime.UtcNow
    },
    cancellationToken);

await repository.SaveChangesAsync(cancellationToken);

                continue;
            }

            if (product.StockQty < item.Quantity)
            {
                return Failure(
                    400,
                    $"Not enough stock for {product.Name}.");
            }

            var linePrice = CalculateLinePrice(
                product.Price,
                item.Quantity,
                customer.MembershipLevel);

            subtotal += linePrice;
            totalItemCount += item.Quantity;

            product.StockQty -= item.Quantity;

            orderLines.Add(new OrderLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Qty = item.Quantity,
                LineTotal = linePrice
            });
        }

        var shipping = CalculateShipping(subtotal);
        var tax = CalculateTax(subtotal, shipping, request.ShippingState);
        var total = Math.Round(
            subtotal + shipping + tax,
            2,
            MidpointRounding.AwayFromZero);

        var order = new Order
        {
            CustomerId = customer.Id,
            CreatedDate = DateTime.UtcNow,
            Subtotal = subtotal,
            Shipping = shipping,
            Tax = tax,
            Total = total,
            Status = "PENDING",
            ItemCount = totalItemCount
        };

        await repository.AddOrderAsync(order, cancellationToken);

        foreach (var line in orderLines)
        {
            order.OrderLines.Add(line);
        }

        var pointsEarned = (int)(total / 10);

        var loyalty = await repository.GetLoyaltyAccountAsync(
            customer.Id,
            cancellationToken);

        if (loyalty is null)
        {
            loyalty = new LoyaltyAccount
            {
                CustomerId = customer.Id,
                Points = 0
            };

            await repository.AddLoyaltyAccountAsync(
                loyalty,
                cancellationToken);
        }

        loyalty.Points += pointsEarned;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promo = await repository.GetPromoCodeAsync(
                request.PromoCode,
                cancellationToken);

            if (promo is not null &&
                promo.IsActive &&
                promo.UsesRemaining > 0)
            {
                promo.UsesRemaining--;
                order.Total = Math.Max(
                    0,
                    order.Total - promo.DiscountAmount);
            }
        }

        var customerOrders = await repository.GetCustomerOrdersAsync(
            customer.Id,
            cancellationToken);

        var lifetimeTotal = customerOrders.Sum(o => o.Total) + order.Total;

        if (lifetimeTotal > 1000 &&
            customer.MembershipLevel != "GOLD")
        {
            customer.MembershipLevel = "GOLD";
        }
        else if (lifetimeTotal > 500 &&
                 customer.MembershipLevel == "BASIC")
        {
            customer.MembershipLevel = "SILVER";
        }

        await repository.AddNotificationAsync(
            new Notification
            {
                CustomerId = customer.Id,
                Message = $"Your order has been placed.",
                SentDate = DateTime.UtcNow
            },
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created order {OrderId} for CustomerId {CustomerId} with total {Total}",
            order.Id,
            customer.Id,
            order.Total);

        var response = new CreateOrderResponse(
            order.Id,
            customer.Name,
            orderLines
                .Select(x => new OrderLineResponse(
                    x.ProductName,
                    x.Qty,
                    x.LineTotal))
                .ToList(),
            order.Subtotal,
            order.Shipping,
            order.Tax,
            order.Total,
            pointsEarned,
            customer.MembershipLevel);

        return new CreateOrderResult(
            true,
            201,
            null,
            response);
    }

    private static decimal CalculateLinePrice(
        decimal price,
        int quantity,
        string membershipLevel)
    {
        var linePrice = price * quantity;

        if (quantity >= 10)
        {
            linePrice *= Bulk10Discount;
        }
        else if (quantity >= 5)
        {
            linePrice *= Bulk5Discount;
        }

        if (membershipLevel == "GOLD")
        {
            linePrice *= GoldDiscount;
        }
        else if (membershipLevel == "SILVER")
        {
            linePrice *= SilverDiscount;
        }

        return linePrice;
    }

    private static decimal CalculateShipping(decimal subtotal)
    {
        if (subtotal < 50)
        {
            return 7.99m;
        }

        if (subtotal < 100)
        {
            return 4.99m;
        }

        return 0m;
    }

    private static decimal CalculateTax(
        decimal subtotal,
        decimal shipping,
        string state)
    {
        var taxRate = state is "OR" or "MT" or "NH"
            ? 0m
            : TaxRate;

        return (subtotal + shipping) * taxRate;
    }

    private static CreateOrderResult Failure(
        int statusCode,
        string error)
    {
        return new CreateOrderResult(
            false,
            statusCode,
            error,
            null);
    }
}