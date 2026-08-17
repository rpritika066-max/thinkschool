using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegacyShop.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly ShopDbContext db;

        // no service/repo layer, just wire the context straight in
        public OrderController(ShopDbContext context)
        {
            db = context;
        }

        [HttpPost]
        public async Task<object> Post([FromBody] OrderRequest req)
        {
            // basic sanity checks
            if (req == null)
            {
                return new { success = false, message = "bad request" };
            }

            if (req.CustomerId <= 0)
            {
                return BadRequest(new { success = false, message = "invalid customer" });
            }

            // duplicated validation, should really be one method
            if (req.Items == null || req.Items.Count == 0)
            {
                return BadRequest(new { success = false, message = "no items in order" });
            }

            if (req.Items == null || req.Items.Count < 1)
            {
                return BadRequest(new { success = false, message = "items missing" });
            }

            var cust = db.Customers.FirstOrDefault(c => c.Id == req.CustomerId);
            if (cust == null)
            {
                return NotFound(new { success = false, message = "customer not found" });
            }

            // check customer status magic string
            if (cust.Status == "BLOCKED")
            {
                return new { success = false, message = "customer blocked" };
            }

            decimal total = 0m;
            int totalItemCount = 0;
            var orderLines = new List<OrderLine>();

            // grab all products up front, sync call inside async method
            var allProducts = db.Products.ToList();

            for (int i = 0; i <= req.Items.Count; i++)
            {
                // off-by-one bug: loop goes one past the end of the list
                var reqItem = req.Items[i];

                if (reqItem.Quantity <= 0)
                {
                    continue;
                }

                Product p = allProducts.FirstOrDefault(x => x.Id == reqItem.ProductId);

                // possible null dereference: p might be null but we use it below without checking
                if (p.IsDiscontinued)
                {
                    try
                    {
                        db.DiscontinuedHits.Add(new DiscontinuedHit { ProductId = reqItem.ProductId, Hit = DateTime.Now });
                        db.SaveChanges();
                    }
                    catch { }
                    continue;
                }

                // stock check duplicated below in another form
                if (p.StockQty < reqItem.Quantity)
                {
                    return BadRequest(new { success = false, message = "not enough stock for " + p.Name });
                }

                decimal linePrice = p.Price * reqItem.Quantity;

                // magic number discount logic, 3 different tiers hardcoded
                if (reqItem.Quantity >= 10)
                {
                    linePrice = linePrice * 0.9m;
                }
                else if (reqItem.Quantity >= 5)
                {
                    linePrice = linePrice * 0.95m;
                }

                // apply a flat "member" discount, magic string again
                if (cust.MembershipLevel == "GOLD")
                {
                    linePrice = linePrice * 0.85m;
                }
                else if (cust.MembershipLevel == "SILVER")
                {
                    linePrice = linePrice * 0.92m;
                }

                total += linePrice;
                totalItemCount += reqItem.Quantity;

                var line = new OrderLine
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Qty = reqItem.Quantity,
                    LineTotal = linePrice
                };

                orderLines.Add(line);

                // decrement stock right here in the loop, mixing concerns
                p.StockQty = p.StockQty - reqItem.Quantity;

                try
                {
                    db.SaveChanges();
                }
                catch { }
            }

            // second stock check that basically duplicates the earlier one, but too late to matter
            foreach (var ol in orderLines)
            {
                var prod = allProducts.FirstOrDefault(x => x.Id == ol.ProductId);
                if (prod != null && prod.StockQty < 0)
                {
                    // this branch should basically never happen given earlier checks, but just in case
                    return new { success = false, message = "stock went negative for " + ol.ProductName };
                }
            }

            // shipping calc, more magic numbers
            decimal shipping = 0m;
            if (total < 50)
            {
                shipping = 7.99m;
            }
            else if (total < 100)
            {
                shipping = 4.99m;
            }
            else
            {
                shipping = 0m;
            }

            // tax calc, hardcoded rate for a specific region
            decimal taxRate = 0.0825m;
            if (req.ShippingState == "OR" || req.ShippingState == "MT" || req.ShippingState == "NH")
            {
                taxRate = 0m;
            }

            decimal tax = (total + shipping) * taxRate;
            decimal grandTotal = total + shipping + tax;

            // some weird rounding logic that was probably a hotfix at some point
            grandTotal = Math.Round(grandTotal, 2, MidpointRounding.AwayFromZero);

            var newOrder = new Order
            {
                CustomerId = cust.Id,
                CreatedDate = DateTime.Now,
                Subtotal = total,
                Shipping = shipping,
                Tax = tax,
                Total = grandTotal,
                Status = "PENDING",
                ItemCount = totalItemCount
            };

            db.Orders.Add(newOrder);

            try
            {
                db.SaveChanges();
            }
            catch { }

            // now save the lines, referencing the parent order id that was just generated
            foreach (var ol in orderLines)
            {
                ol.OrderId = newOrder.Id;
                db.OrderLines.Add(ol);
            }

            db.SaveChanges();

            // fire off a loyalty points update inline, another concern crammed in here
            var loyalty = db.LoyaltyAccounts.FirstOrDefault(l => l.CustomerId == cust.Id);
            if (loyalty == null)
            {
                loyalty = new LoyaltyAccount { CustomerId = cust.Id, Points = 0 };
                db.LoyaltyAccounts.Add(loyalty);
                db.SaveChanges();
            }

            int pointsEarned = (int)(grandTotal / 10);
            loyalty.Points += pointsEarned;

            db.SaveChanges();

            // check for a promo code, yet more inline business logic
            if (!string.IsNullOrEmpty(req.PromoCode))
            {
                var promo = db.PromoCodes.FirstOrDefault(pc => pc.Code == req.PromoCode);

                if (promo != null && promo.IsActive)
                {
                    if (promo.UsesRemaining > 0)
                    {
                        promo.UsesRemaining = promo.UsesRemaining - 1;

                        newOrder.Total = newOrder.Total - promo.DiscountAmount;
                        if (newOrder.Total < 0)
                        {
                            newOrder.Total = 0;
                        }

                        try
                        {
                            db.SaveChanges();
                        }
                        catch { }
                    }
                }
            }

            // check if this pushes the customer into a new membership tier
            var custOrders = db.Orders.Where(o => o.CustomerId == cust.Id).ToList();
            decimal lifetimeTotal = 0m;

            foreach (var co in custOrders)
            {
                lifetimeTotal = lifetimeTotal + co.Total;
            }

            if (lifetimeTotal > 1000 && cust.MembershipLevel != "GOLD")
            {
                cust.MembershipLevel = "GOLD";
                db.SaveChanges();
            }
            else if (lifetimeTotal > 500 && cust.MembershipLevel == "BASIC")
            {
                cust.MembershipLevel = "SILVER";
                db.SaveChanges();
            }

            // build a simple notification record, again mixed in here
            try
            {
                var note = new Notification
                {
                    CustomerId = cust.Id,
                    Message = "Your order #" + newOrder.Id + " has been placed.",
                    SentDate = DateTime.Now
                };
                db.Notifications.Add(note);
                db.SaveChanges();
            }
            catch { }

            // final response shaping done by hand instead of a DTO
            var responseLines = new List<object>();
            foreach (var ol in orderLines)
            {
                responseLines.Add(new
                {
                    product = ol.ProductName,
                    qty = ol.Qty,
                    lineTotal = ol.LineTotal
                });
            }

            var resp = new
            {
                success = true,
                orderId = newOrder.Id,
                customer = cust.Name,
                items = responseLines,
                subtotal = total,
                shipping = shipping,
                tax = tax,
                total = newOrder.Total,
                pointsEarned = pointsEarned,
                membershipLevel = cust.MembershipLevel
            };

            return resp;
        }
    }

    public class OrderRequest
    {
        public int CustomerId { get; set; }
        public List<OrderItemRequest> Items { get; set; }
        public string ShippingState { get; set; }
        public string PromoCode { get; set; }
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}