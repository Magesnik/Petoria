using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Room;
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
    public async Task<ActionResult<IEnumerable<RoomTypeResponseDto>>> GetRoomTypes(int hotelId)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotelId)
            .OrderBy(rt => rt.PricePerNight)
            .Select(rt => new RoomTypeResponseDto
            {
                Id = rt.Id,
                HotelId = rt.HotelId,
                Name = rt.Name,
                Description = rt.Description,
                PricePerNight = rt.PricePerNight,
                Capacity = rt.Capacity,
                TotalRooms = rt.TotalRooms,
                ImageUrl = rt.ImageUrl,
                CreatedAt = rt.CreatedAt,
                UpdatedAt = rt.UpdatedAt
            })
            .ToListAsync();

        return Ok(roomTypes);
    }

    // GET: api/hotels/5/rooms/1
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomTypeResponseDto>> GetRoomType(int hotelId, int id)
    {
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == id && rt.HotelId == hotelId);

        if (roomType == null)
        {
            return NotFound();
        }

        return Ok(new RoomTypeResponseDto
        {
            Id = roomType.Id,
            HotelId = roomType.HotelId,
            Name = roomType.Name,
            Description = roomType.Description,
            PricePerNight = roomType.PricePerNight,
            Capacity = roomType.Capacity,
            TotalRooms = roomType.TotalRooms,
            ImageUrl = roomType.ImageUrl,
            CreatedAt = roomType.CreatedAt,
            UpdatedAt = roomType.UpdatedAt
        });
    }

    // POST: api/hotels/5/rooms
    [HttpPost]
    [Authorize(Roles = Petoria.Constants.Roles.Admin)]
    public async Task<ActionResult<RoomTypeResponseDto>> CreateRoomType(int hotelId, CreateRoomTypeDto dto)
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

        // Map DTO → Entity
        var roomType = new RoomType
        {
            HotelId = hotelId,
            Name = dto.Name,
            Description = dto.Description,
            PricePerNight = dto.PricePerNight,
            Capacity = dto.Capacity,
            TotalRooms = dto.TotalRooms,
            ImageUrl = dto.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        // Map Entity → Response DTO
        var response = new RoomTypeResponseDto
        {
            Id = roomType.Id,
            HotelId = roomType.HotelId,
            Name = roomType.Name,
            Description = roomType.Description,
            PricePerNight = roomType.PricePerNight,
            Capacity = roomType.Capacity,
            TotalRooms = roomType.TotalRooms,
            ImageUrl = roomType.ImageUrl,
            CreatedAt = roomType.CreatedAt,
            UpdatedAt = roomType.UpdatedAt
        };

        return CreatedAtAction(nameof(GetRoomType), new { hotelId, id = roomType.Id }, response);
    }

    // PUT: api/hotels/5/rooms/1
    [HttpPut("{id}")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin)]
    public async Task<IActionResult> UpdateRoomType(int hotelId, int id, UpdateRoomTypeDto dto)
    {
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

        // Map DTO → Entity (update)
        existingRoom.Name = dto.Name;
        existingRoom.Description = dto.Description;
        existingRoom.PricePerNight = dto.PricePerNight;
        existingRoom.Capacity = dto.Capacity;
        existingRoom.TotalRooms = dto.TotalRooms;
        existingRoom.ImageUrl = dto.ImageUrl;
        existingRoom.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/hotels/5/rooms/1
    [HttpDelete("{id}")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin)]
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
