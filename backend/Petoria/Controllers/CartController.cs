using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.Contracts;
using Petoria.Core.DTOs.Cart;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPricingService _pricingService;

    public CartController(ApplicationDbContext context, IPricingService pricingService)
    {
        _context = context;
        _pricingService = pricingService;
    }

    // GET: api/cart - Get current user's cart items
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemResponseDto>>> GetCart()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var items = await _context.CartItems
            .Include(c => c.Hotel)
            .Include(c => c.RoomType)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var responseItems = items.Select(c => new CartItemResponseDto
        {
            Id = c.Id,
            HotelId = c.HotelId,
            HotelName = c.Hotel != null ? c.Hotel.Name : "",
            HotelImageUrl = c.Hotel != null ? c.Hotel.ImageUrl : "",
            RoomTypeId = c.RoomTypeId,
            RoomTypeName = c.RoomType != null ? c.RoomType.Name : "",
            CheckInDate = c.CheckInDate,
            CheckOutDate = c.CheckOutDate,
            NumberOfNights = (int)(c.CheckOutDate - c.CheckInDate).TotalDays,
            NumberOfRooms = c.NumberOfRooms,
            PricePerNight = c.RoomType != null ? c.RoomType.PricePerNight : 0,
            TotalPrice = c.TotalPrice,
            OriginalPrice = c.OriginalPrice,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Ok(responseItems);
    }

    // POST: api/cart - Add item to cart
    [HttpPost]
    public async Task<ActionResult<CartItemResponseDto>> AddToCart(AddToCartDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var hotel = await _context.Hotels.FindAsync(request.HotelId);
        if (hotel == null) return NotFound(new { message = "Hotel not found" });

        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == request.RoomTypeId && rt.HotelId == request.HotelId);
        if (roomType == null) return NotFound(new { message = "Room type not found" });

        if (request.CheckOutDate <= request.CheckInDate)
            return BadRequest(new { message = "Check-out date must be after check-in date" });

        var priceResult = await _pricingService.CalculatePriceAsync(
            request.RoomTypeId,
            request.CheckInDate,
            request.CheckOutDate,
            request.NumberOfRooms,
            userId);

        var cartItem = new CartItem
        {
            UserId = userId,
            HotelId = request.HotelId,
            RoomTypeId = request.RoomTypeId,
            CheckInDate = request.CheckInDate.Date,
            CheckOutDate = request.CheckOutDate.Date,
            NumberOfRooms = request.NumberOfRooms,
            TotalPrice = priceResult.TotalPrice,
            OriginalPrice = priceResult.OriginalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCart), new CartItemResponseDto
        {
            Id = cartItem.Id,
            HotelId = cartItem.HotelId,
            HotelName = hotel.Name,
            HotelImageUrl = hotel.ImageUrl ?? "",
            RoomTypeId = cartItem.RoomTypeId,
            RoomTypeName = roomType.Name,
            CheckInDate = cartItem.CheckInDate,
            CheckOutDate = cartItem.CheckOutDate,
            NumberOfNights = (int)(cartItem.CheckOutDate - cartItem.CheckInDate).TotalDays,
            NumberOfRooms = cartItem.NumberOfRooms,
            PricePerNight = roomType.PricePerNight,
            TotalPrice = cartItem.TotalPrice,
            OriginalPrice = cartItem.OriginalPrice,
            CreatedAt = cartItem.CreatedAt
        });
    }

    // DELETE: api/cart/{id} - Remove item from cart
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var cartItem = await _context.CartItems.FindAsync(id);
        if (cartItem == null) return NotFound();
        if (cartItem.UserId != userId) return Forbid();

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Item removed from cart" });
    }

    // DELETE: api/cart - Clear entire cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cart cleared" });
    }

    // GET: api/cart/count - Get cart item count
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCartCount()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var count = await _context.CartItems.CountAsync(c => c.UserId == userId);
        return Ok(count);
    }
}
