using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // GET: api/admin/users
    [HttpGet("users")]
    public async Task<ActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userStats = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var favoritesCount = await _context.Favorites.CountAsync(f => f.UserId == user.Id);
            var reservationsCount = await _context.Reservations.CountAsync(r => r.UserId == user.Id);
            var totalSpent = await _context.Reservations
                .Where(r => r.UserId == user.Id && r.Status == "Completed")
                .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;
            var hotelsCreated = await _context.Hotels.CountAsync(h => h.CreatedById == user.Id);

            userStats.Add(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                avatarUrl = user.AvatarUrl,
                roles = roles,
                favoritesCount,
                reservationsCount,
                totalSpent,
                hotelsCreated
            });
        }

        return Ok(userStats);
    }

    // GET: api/admin/users/{id}
    [HttpGet("users/{id}")]
    public async Task<ActionResult> GetUserDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Get user's favorites with hotel details
        var favorites = await _context.Favorites
            .Where(f => f.UserId == id)
            .Include(f => f.Hotel)
            .Select(f => new
            {
                id = f.Id,
                hotelId = f.HotelId,
                hotelName = f.Hotel!.Name,
                hotelCity = f.Hotel.City,
                hotelCountry = f.Hotel.Country,
                hotelImageUrl = f.Hotel.ImageUrl,
                createdAt = f.CreatedAt
            })
            .ToListAsync();

        // Get user's reservations with hotel details
        var reservations = await _context.Reservations
            .Where(r => r.UserId == id)
            .Include(r => r.Hotel)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                id = r.Id,
                hotelId = r.HotelId,
                hotelName = r.Hotel!.Name,
                hotelCity = r.Hotel.City,
                hotelImageUrl = r.Hotel.ImageUrl,
                checkInDate = r.CheckInDate,
                checkOutDate = r.CheckOutDate,
                totalPrice = r.TotalPrice,
                status = r.Status,
                createdAt = r.CreatedAt
            })
            .ToListAsync();

        // Get hotels created by user
        var hotelsCreated = await _context.Hotels
            .Where(h => h.CreatedById == id)
            .Select(h => new
            {
                id = h.Id,
                name = h.Name,
                city = h.City,
                country = h.Country,
                imageUrl = h.ImageUrl,
                createdAt = h.CreatedAt
            })
            .ToListAsync();

        var totalSpent = await _context.Reservations
            .Where(r => r.UserId == id && r.Status == "Completed")
            .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            avatarUrl = user.AvatarUrl,
            roles = roles,
            totalSpent,
            favorites,
            reservations,
            hotelsCreated
        });
    }

    // PUT: api/admin/users/{id}/promote
    [HttpPut("users/{id}/promote")]
    public async Task<ActionResult> PromoteToAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Check if already admin
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return BadRequest("User is already an Admin");
        }

        var result = await _userManager.AddToRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = $"User {user.Email} has been promoted to Admin" });
    }

    // DELETE: api/admin/users/{id}/demote
    [HttpDelete("users/{id}/demote")]
    public async Task<ActionResult> DemoteFromAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Cannot demote SuperAdmin
        if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            return BadRequest("Cannot demote a SuperAdmin");
        }

        // Check if user is admin
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return BadRequest("User is not an Admin");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = $"User {user.Email} has been demoted from Admin" });
    }

    // GET: api/admin/stats
    [HttpGet("stats")]
    public async Task<ActionResult> GetDashboardStats()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var adminUsers = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
        var superAdminUsers = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Count;
        var totalFavorites = await _context.Favorites.CountAsync();
        var totalReservations = await _context.Reservations.CountAsync();
        var totalRevenue = await _context.Reservations
            .Where(r => r.Status == "Completed")
            .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;
        var totalHotels = await _context.Hotels.CountAsync();

        return Ok(new
        {
            totalUsers,
            adminUsers = adminUsers - superAdminUsers, // Exclude super admins from admin count
            superAdminUsers,
            totalFavorites,
            totalReservations,
            totalRevenue,
            totalHotels
        });
    }
}
