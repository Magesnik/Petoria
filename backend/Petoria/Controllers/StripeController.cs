using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StripeController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public StripeController(IConfiguration configuration)
    {
        _configuration = configuration;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    // GET: api/stripe/config
    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        return Ok(new { publishableKey = _configuration["Stripe:PublishableKey"] });
    }

    // POST: api/stripe/create-checkout-session
    [HttpPost("create-checkout-session")]
    [Authorize]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
    {
        // Bulgaria's official BGN→EUR fixed rate (1 EUR = 1.95583 BGN)
        const decimal bgnToEur = 1.95583m;

        var lineItems = request.Items.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = "eur",
                UnitAmount = (long)Math.Round(item.TotalPrice / bgnToEur * 100), // EUR cents
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = $"{item.HotelName} – {item.RoomTypeName}",
                    Description = $"Check-in: {item.CheckInDate:dd.MM.yyyy} → Check-out: {item.CheckOutDate:dd.MM.yyyy} | {item.NumberOfRooms} стая/и"
                }
            },
            Quantity = 1
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = $"{request.FrontendBaseUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{request.FrontendBaseUrl}/payment/cancel",
            Metadata = new Dictionary<string, string>
            {
                { "userId", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "" }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return Ok(new { url = session.Url, sessionId = session.Id });
    }
}

public class CreateCheckoutSessionRequest
{
    public List<CheckoutItem> Items { get; set; } = new();
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}

public class CheckoutItem
{
    public int CartItemId { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = "";
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = "";
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; }
    public decimal TotalPrice { get; set; }
}
