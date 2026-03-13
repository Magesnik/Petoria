using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Admin;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Petoria.Constants.Roles.SuperAdmin)]
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
    public async Task<ActionResult<IEnumerable<UserStatsResponseDto>>> GetAllUsers()
    {
        var users = await _userManager.Users.Where(u => u.EmailConfirmed).ToListAsync();
        var userStats = new List<UserStatsResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var favoritesCount = await _context.Favorites.CountAsync(f => f.UserId == user.Id);
            var reservationsCount = await _context.Reservations.CountAsync(r => r.UserId == user.Id);
            var totalSpent = await _context.Reservations
                .Where(r => r.UserId == user.Id && (r.Status == "Completed" || r.Status == "Confirmed"))
                .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;
            var hotelsCreated = await _context.Hotels.CountAsync(h => h.CreatedById == user.Id);
            var commentsCount = await _context.Comments.CountAsync(c => c.UserId == user.Id);
            var reviewsCount = await _context.HotelReviews.CountAsync(r => r.UserId == user.Id);

            userStats.Add(new UserStatsResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles.ToList(),
                IsBlocked = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                FavoritesCount = favoritesCount,
                ReservationsCount = reservationsCount,
                TotalSpent = totalSpent,
                HotelsCreated = hotelsCreated,
                CommentsCount = commentsCount,
                ReviewsCount = reviewsCount
            });
        }

        return Ok(userStats);
    }

    // GET: api/admin/users/{id}
    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDetailsResponseDto>> GetUserDetails(string id)
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
            .Select(f => new UserFavoriteDto
            {
                Id = f.Id,
                HotelId = f.HotelId,
                HotelName = f.Hotel!.Name,
                HotelCity = f.Hotel.City,
                HotelCountry = f.Hotel.Country,
                HotelImageUrl = f.Hotel.ImageUrl,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        // Get user's reservations with hotel details
        var reservations = await _context.Reservations
            .Where(r => r.UserId == id)
            .Include(r => r.Hotel)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new UserReservationDto
            {
                Id = r.Id,
                HotelId = r.HotelId,
                HotelName = r.Hotel!.Name,
                HotelCity = r.Hotel.City,
                HotelImageUrl = r.Hotel.ImageUrl,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        // Get hotels created by user
        var hotelsCreated = await _context.Hotels
            .Where(h => h.CreatedById == id)
            .Select(h => new UserHotelDto
            {
                Id = h.Id,
                Name = h.Name,
                City = h.City,
                Country = h.Country,
                ImageUrl = h.ImageUrl,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();

        // Get user's comments with hotel and rating details
        var comments = await _context.Comments
            .Where(c => c.UserId == id)
            .Include(c => c.Hotel)
            .Include(c => c.Ratings)
            .Include(c => c.Replies)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new UserCommentDto
            {
                Id = c.Id,
                Text = c.Text,
                HotelId = c.HotelId,
                HotelName = c.Hotel!.Name,
                ParentCommentId = c.ParentCommentId,
                CreatedAt = c.CreatedAt,
                LikesCount = c.Ratings.Count(r => r.IsLike),
                DislikesCount = c.Ratings.Count(r => !r.IsLike),
                RepliesCount = c.Replies.Count
            })
            .ToListAsync();

        // Get user's reviews with hotel details
        var reviews = await _context.HotelReviews
            .Where(r => r.UserId == id)
            .Include(r => r.Hotel)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new UserReviewDto
            {
                Id = r.Id,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                HotelId = r.HotelId,
                HotelName = r.Hotel!.Name,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var totalSpent = await _context.Reservations
            .Where(r => r.UserId == id && (r.Status == "Completed" || r.Status == "Confirmed"))
            .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;

        // Map Entity → Response DTO
        return Ok(new UserDetailsResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList(),
            IsBlocked = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
            TotalSpent = totalSpent,
            Favorites = favorites,
            Reservations = reservations,
            HotelsCreated = hotelsCreated,
            Comments = comments,
            Reviews = reviews
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

    // PUT: api/admin/users/{id}/promote-super
    [HttpPut("users/{id}/promote-super")]
    public async Task<ActionResult> PromoteToSuperAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (await _userManager.IsInRoleAsync(user, Petoria.Constants.Roles.SuperAdmin))
            return BadRequest("User is already a SuperAdmin");

        if (!await _userManager.IsInRoleAsync(user, Petoria.Constants.Roles.Admin))
            await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.Admin);

        var result = await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.SuperAdmin);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"User {user.Email} has been promoted to SuperAdmin" });
    }

    // DELETE: api/admin/users/{id}/demote-super
    [HttpDelete("users/{id}/demote-super")]
    public async Task<ActionResult> DemoteFromSuperAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (!await _userManager.IsInRoleAsync(user, Petoria.Constants.Roles.SuperAdmin))
            return BadRequest("User is not a SuperAdmin");

        var result = await _userManager.RemoveFromRoleAsync(user, Petoria.Constants.Roles.SuperAdmin);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"User {user.Email} has been demoted from SuperAdmin" });
    }


    // DELETE: api/admin/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == currentUserId)
            return BadRequest("Cannot delete yourself");

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (await _userManager.IsInRoleAsync(user, Petoria.Constants.Roles.SuperAdmin))
            return BadRequest("Cannot delete a SuperAdmin");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"User {user.Email} has been deleted" });
    }

    // PUT: api/admin/users/{id}/block
    [HttpPut("users/{id}/block")]
    public async Task<ActionResult> BlockUser(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == currentUserId)
            return BadRequest("Cannot block yourself");

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (await _userManager.IsInRoleAsync(user, Petoria.Constants.Roles.SuperAdmin))
            return BadRequest("Cannot block a SuperAdmin");

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return Ok(new { message = $"User {user.Email} has been blocked" });
    }

    // PUT: api/admin/users/{id}/unblock
    [HttpPut("users/{id}/unblock")]
    public async Task<ActionResult> UnblockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        await _userManager.SetLockoutEndDateAsync(user, null);

        return Ok(new { message = $"User {user.Email} has been unblocked" });
    }

    // GET: api/admin/stats
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsResponseDto>> GetDashboardStats()
    {
        var totalUsers = await _userManager.Users.CountAsync(u => u.EmailConfirmed);
        var adminUsers = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
        var superAdminUsers = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Count;
        var totalFavorites = await _context.Favorites.CountAsync();
        var totalReservations = await _context.Reservations.CountAsync();
        var totalRevenue = await _context.Reservations
            .SumAsync(r => 
                (r.Status == "Completed" || r.Status == "Confirmed") ? r.TotalPrice : 
                (r.Status == "Cancelled") ? r.RetainedAmount : 0
            );
        var totalHotels = await _context.Hotels.CountAsync();
        var totalComments = await _context.Comments.CountAsync();
        var totalReviews = await _context.HotelReviews.CountAsync();

        return Ok(new DashboardStatsResponseDto
        {
            TotalUsers = totalUsers,
            AdminUsers = adminUsers - superAdminUsers, // Exclude super admins from admin count
            SuperAdminUsers = superAdminUsers,
            TotalFavorites = totalFavorites,
            TotalReservations = totalReservations,
            TotalRevenue = totalRevenue,
            TotalHotels = totalHotels,
            TotalComments = totalComments,
            TotalReviews = totalReviews
        });
    }

    // GET: api/admin/moderators
    [HttpGet("moderators")]
    public async Task<ActionResult<IEnumerable<AdminModeratorDto>>> GetAllModerators()
    {
        var moderators = await _context.HotelModerators
            .Include(hm => hm.Hotel)
            .Include(hm => hm.User)
            .Select(hm => new AdminModeratorDto
            {
                Id = hm.Id,
                HotelId = hm.HotelId,
                HotelName = hm.Hotel.Name,
                UserId = hm.UserId,
                UserEmail = hm.User.Email!,
                UserFullName = $"{hm.User.FirstName} {hm.User.LastName}",
                AddedAt = hm.CreatedAt
            })
            .ToListAsync();

        return Ok(moderators);
    }

    // DELETE: api/admin/moderators/{id}
    [HttpDelete("moderators/{id}")]
    public async Task<IActionResult> RemoveModeratorRole(int id)
    {
        var moderator = await _context.HotelModerators.FindAsync(id);
        if (moderator == null)
        {
            return NotFound("Moderator assignment not found");
        }

        _context.HotelModerators.Remove(moderator);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Moderator role removed successfully" });
    }

    // GET: api/admin/hotels
    [HttpGet("hotels")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllHotelsForAdmin()
    {
        var hotels = await _context.Hotels
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Country,
                h.IsAvailable,
                h.IsSuspendedBySuperAdmin,
                h.CreatedAt,
                OwnerEmail = _context.Users
                    .Where(u => u.Id == h.CreatedById)
                    .Select(u => u.Email)
                    .FirstOrDefault()
            })
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return Ok(hotels);
    }

    // POST: api/admin/hotels/{id}/toggle-suspend
    [HttpPost("hotels/{id}/toggle-suspend")]
    public async Task<IActionResult> ToggleHotelSuspend(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        hotel.IsSuspendedBySuperAdmin = !hotel.IsSuspendedBySuperAdmin;
        hotel.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            hotelId = hotel.Id,
            isSuspendedBySuperAdmin = hotel.IsSuspendedBySuperAdmin,
            message = hotel.IsSuspendedBySuperAdmin
                ? $"Хотел '{hotel.Name}' е деактивиран."
                : $"Хотел '{hotel.Name}' е активиран."
        });
    }
}
