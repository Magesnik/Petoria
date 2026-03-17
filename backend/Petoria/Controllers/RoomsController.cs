using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Room;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

/// <summary>
/// Контролер за типове стаи: CRUD в рамките на хотел
/// </summary>
[Route("api/hotels/{hotelId}/rooms")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Връща всички типове стаи за даден хотел
    /// </summary>
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

    /// <summary>
    /// Връща конкретен тип стая по идентификатор в рамките на хотел
    /// </summary>
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

    /// <summary>
    /// Създава нов тип стая в хотел
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Petoria.Constants.Roles.Admin + "," + Petoria.Constants.Roles.SuperAdmin + "," + Petoria.Constants.Roles.HotelModerator)]
    public async Task<ActionResult<RoomTypeResponseDto>> CreateRoomType(int hotelId, CreateRoomTypeDto dto)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Проверка дали потребителят е собственик на хотела или SuperAdmin
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var isModerator = await _context.HotelModerators.AnyAsync(hm => hm.HotelId == hotelId && hm.UserId == userId);
        if (!isSuperAdmin && hotel.CreatedById != userId && !isModerator)
        {
            return Forbid("You can only add rooms to hotels that you created or moderate");
        }

        // Преобразуване на DTO → Entity
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

        // Преобразуване на Entity → Response DTO
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

    /// <summary>
    /// Обновява съществуващ тип стая по идентификатор
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin + "," + Petoria.Constants.Roles.SuperAdmin + "," + Petoria.Constants.Roles.HotelModerator)]
    public async Task<IActionResult> UpdateRoomType(int hotelId, int id, UpdateRoomTypeDto dto)
    {
        var existingRoom = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .FirstOrDefaultAsync(rt => rt.Id == id && rt.HotelId == hotelId);

        if (existingRoom == null)
        {
            return NotFound();
        }

        // Проверка на оторизацията
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var isModerator = await _context.HotelModerators.AnyAsync(hm => hm.HotelId == hotelId && hm.UserId == userId);

        if (!isSuperAdmin && existingRoom.Hotel?.CreatedById != userId && !isModerator)
        {
            return Forbid("You can only edit rooms in hotels that you created or moderate");
        }

        // Преобразуване на DTO → Entity (обновяване)
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

    /// <summary>
    /// Изтрива тип стая по идентификатор
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin + "," + Petoria.Constants.Roles.SuperAdmin + "," + Petoria.Constants.Roles.HotelModerator)]
    public async Task<IActionResult> DeleteRoomType(int hotelId, int id)
    {
        var roomType = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .FirstOrDefaultAsync(rt => rt.Id == id && rt.HotelId == hotelId);

        if (roomType == null)
        {
            return NotFound();
        }

        // Проверка на оторизацията
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var isModerator = await _context.HotelModerators.AnyAsync(hm => hm.HotelId == hotelId && hm.UserId == userId);

        if (!isSuperAdmin && roomType.Hotel?.CreatedById != userId && !isModerator)
        {
            return Forbid("You can only delete rooms in hotels that you created or moderate");
        }

        _context.RoomTypes.Remove(roomType);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
