using System.Security.Claims;
using Enhanzer.Assignment.Application.PurchaseBills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Assignment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/purchase-bills")]
public sealed class PurchaseBillsController(PurchaseBillService purchaseBillService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SavePurchaseBillResponseDto>> Save(
        SavePurchaseBillRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var response = await purchaseBillService.SaveAsync(email, request, cancellationToken);
        return Ok(response);
    }
}
