using LegacyShop.Api.DTOs;
using LegacyShop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LegacyShop.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateOrderResponse>> Post(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await orderService.CreateOrderAsync(
            request,
            cancellationToken);

        if (!result.Success)
        {
            if (result.StatusCode == StatusCodes.Status404NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = result.Error
                });
            }

            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid order",
                Detail = result.Error
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Response);
    }
}