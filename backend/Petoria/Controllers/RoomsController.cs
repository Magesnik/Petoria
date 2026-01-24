using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/hotels/{hotelId}/rooms")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/hotels/5/rooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomType>>> GetRoomTypes(int hotelId)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotelId)
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();

        return Ok(roomTypes);
    }

    // GET: api/hotels/5/rooms/1
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomType>> GetRoomType(int hotelId, int id)
    {
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == id && rt.HotelId == hotelId);

        if (roomType == null)
        {
            return NotFound();
        }

        return Ok(roomType);
    }

    // POST: api/hotels/5/rooms
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomType>> CreateRoomType(int hotelId, RoomType roomType)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Check if user owns this hotel or is SuperAdmin
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin && hotel.CreatedById != userId)
        {
            return Forbid("You can only add rooms to hotels that you created");
        }

        roomType.HotelId = hotelId;
        roomType.CreatedAt = DateTime.UtcNow;
        roomType.UpdatedAt = DateTime.UtcNow;

        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoomType), new { hotelId, id = roomType.Id }, roomType);
    }

    // PUT: api/hotels/5/rooms/1
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRoomType(int hotelId, int id, RoomType roomType)
    {
        if (id != roomType.Id)
        {
            return BadRequest();
        }

        var existingRoom = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .FirstOrDefaultAsync(rt => rt.Id == id && rt.HotelId == hotelId);

        if (existingRoom == null)
        {
            return NotFound();
        }

        // Check authorization
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin && existingRoom.Hotel?.CreatedById != userId)
        {
            return Forbid("You can only edit rooms in hotels that you created");
        }

        existingRoom.Name = roomType.Name;
        existingRoom.Description = roomType.Description;
        existingRoom.PricePerNight = roomType.PricePerNight;
        existingRoom.Capacity = roomType.Capacity;
        existingRoom.TotalRooms = roomType.TotalRooms;
        existingRoom.ImageUrl = roomType.ImageUrl;
        existingRoom.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/hotels/5/rooms/1
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoomType(int hotelId, int id)
    {
        var roomType = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .FirstOrDefaultAsync(rt => rt.Id == id && rt.HotelId == hotelId);

        if (roomType == null)
        {
            return NotFound();
        }

        // Check authorization
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin && roomType.Hotel?.CreatedById != userId)
        {
            return Forbid("You can only delete rooms in hotels that you created");
        }

        _context.RoomTypes.Remove(roomType);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
