namespace Petoria.Core.DTOs.Admin;

/// <summary>
/// DTO за детайлна информация на потребител (за SuperAdmin).
/// Включва списъци с любими, резервации, хотели и коментари.
/// </summary>
public class UserDetailsResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsBlocked { get; set; }
    public decimal TotalSpent { get; set; }
    public List<UserFavoriteDto> Favorites { get; set; } = new();
    public List<UserReservationDto> Reservations { get; set; } = new();
    public List<UserHotelDto> HotelsCreated { get; set; } = new();
    public List<UserCommentDto> Comments { get; set; } = new();
    public List<UserReviewDto> Reviews { get; set; } = new();
}

/// <summary>
/// Вложен DTO за любими хотели в детайлния потребителски профил.
/// </summary>
public class UserFavoriteDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string HotelCity { get; set; } = string.Empty;
    public string HotelCountry { get; set; } = string.Empty;
    public string HotelImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Вложен DTO за резервации в детайлния потребителски профил.
/// </summary>
public class UserReservationDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string HotelCity { get; set; } = string.Empty;
    public string HotelImageUrl { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Вложен DTO за хотели създадени от потребителя.
/// </summary>
public class UserHotelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Вложен DTO за коментари на потребителя.
/// </summary>
public class UserCommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public int RepliesCount { get; set; }
}

/// <summary>
/// Вложен DTO за ревюта на потребителя.
/// </summary>
public class UserReviewDto
{
    public int Id { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
