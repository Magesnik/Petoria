using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Petoria.Constants;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Infrastructure.Data;

/// <summary>
/// Seeds the database with demo data when running for the first time.
/// Safe to call on every startup – it checks before inserting.
/// </summary>
public static class DatabaseSeeder
{
    // -----------------------------------------------------------------------
    // Public entry-point
    // -----------------------------------------------------------------------
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Only seed when the Hotels table is completely empty
        if (await db.Hotels.AnyAsync())
            return;

        // ----------------------------------------------------------------
        // 1. Users  (password = "Test123!")
        // ----------------------------------------------------------------
        var users = await SeedUsersAsync(userManager, roleManager);

        // ----------------------------------------------------------------
        // 2. Hotels
        // ----------------------------------------------------------------
        var hotels = SeedHotels(users);
        await db.Hotels.AddRangeAsync(hotels);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 3. Room Types (must happen after hotels have IDs)
        // ----------------------------------------------------------------
        var roomTypes = SeedRoomTypes(hotels);
        await db.RoomTypes.AddRangeAsync(roomTypes);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 4. Room Availability (60 days from today)
        // ----------------------------------------------------------------
        var availabilities = SeedRoomAvailability(roomTypes);
        await db.RoomAvailabilities.AddRangeAsync(availabilities);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 5. Room Discounts
        // ----------------------------------------------------------------
        var discounts = SeedRoomDiscounts(roomTypes);
        await db.RoomDiscounts.AddRangeAsync(discounts);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 6. Reservations
        // ----------------------------------------------------------------
        var reservations = SeedReservations(users, hotels, roomTypes);
        await db.Reservations.AddRangeAsync(reservations);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 7. Hotel Reviews
        // ----------------------------------------------------------------
        var reviews = SeedHotelReviews(users, hotels);
        await db.HotelReviews.AddRangeAsync(reviews);
        await db.SaveChangesAsync();

        // Update hotel ratings based on reviews
        foreach (var hotel in hotels)
        {
            var hotelReviews = reviews.Where(r => r.HotelId == hotel.Id).ToList();
            if (hotelReviews.Any())
                hotel.Rating = Math.Round((decimal)hotelReviews.Average(r => r.Rating), 1);
        }
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 8. Comments (+ replies)
        // ----------------------------------------------------------------
        var comments = SeedComments(users, hotels);
        await db.Comments.AddRangeAsync(comments);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 9. Comment Ratings (likes/dislikes)
        // ----------------------------------------------------------------
        var commentRatings = SeedCommentRatings(users, comments);
        await db.CommentRatings.AddRangeAsync(commentRatings);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 10. Favorites (liked hotels)
        // ----------------------------------------------------------------
        var favorites = SeedFavorites(users, hotels);
        await db.Favorites.AddRangeAsync(favorites);
        await db.SaveChangesAsync();

        // ----------------------------------------------------------------
        // 11. Promo Codes
        // ----------------------------------------------------------------
        var promoCodes = SeedPromoCodes(hotels);
        await db.PromoCodes.AddRangeAsync(promoCodes);
        await db.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // 1. Users
    // -----------------------------------------------------------------------
    private static async Task<List<ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { Roles.Admin, Roles.User, Roles.HotelModerator };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var usersToCreate = new[]
        {
            new ApplicationUser
            {
                UserName = "ivan.petrov@example.com",
                Email = "ivan.petrov@example.com",
                EmailConfirmed = true,
                FirstName = "Иван",
                LastName = "Петров",
                Language = "bg",
                Currency = "BGN",
                Theme = "light"
            },
            new ApplicationUser
            {
                UserName = "maria.georgieva@example.com",
                Email = "maria.georgieva@example.com",
                EmailConfirmed = true,
                FirstName = "Мария",
                LastName = "Георгиева",
                Language = "bg",
                Currency = "BGN",
                Theme = "dark"
            },
            new ApplicationUser
            {
                UserName = "stoyan.nikolov@example.com",
                Email = "stoyan.nikolov@example.com",
                EmailConfirmed = true,
                FirstName = "Стоян",
                LastName = "Николов",
                Language = "en",
                Currency = "EUR",
                Theme = "light"
            },
            new ApplicationUser
            {
                UserName = "elena.dimitrova@example.com",
                Email = "elena.dimitrova@example.com",
                EmailConfirmed = true,
                FirstName = "Елена",
                LastName = "Димитрова",
                Language = "bg",
                Currency = "BGN",
                Theme = "light"
            },
            new ApplicationUser
            {
                UserName = "petar.stoyanov@example.com",
                Email = "petar.stoyanov@example.com",
                EmailConfirmed = true,
                FirstName = "Петър",
                LastName = "Стоянов",
                Language = "bg",
                Currency = "BGN",
                Theme = "dark"
            }
        };

        var createdUsers = new List<ApplicationUser>();

        foreach (var user in usersToCreate)
        {
            var existing = await userManager.FindByEmailAsync(user.Email!);
            if (existing != null)
            {
                createdUsers.Add(existing);
                continue;
            }

            var result = await userManager.CreateAsync(user, "Test123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.User);
                createdUsers.Add(user);
            }
        }

        return createdUsers;
    }

    // -----------------------------------------------------------------------
    // 2. Hotels
    // -----------------------------------------------------------------------
    private static List<Hotel> SeedHotels(List<ApplicationUser> users)
    {
        var adminId = users.First().Id;
        var now = DateTime.UtcNow;

        return new List<Hotel>
        {
            new Hotel
            {
                Name = "Гранд Хотел София",
                Description = "Луксозен 5-звезден хотел в сърцето на София, предлагащ изключително обслужване, СПА център, ресторант с висока кухня и панорамна гледка към Витоша. Идеален за бизнес пътувания и романтични почивки.",
                Location = "ул. Цар Освободител 1, София 1000",
                City = "София",
                Country = "България",
                Latitude = 42.6977m,
                Longitude = 23.3219m,
                Rating = 4.8m,
                StarRating = 5,
                ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
                Images = "[\"https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800\",\"https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800\",\"https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800\"]",
                Amenities = "[\"WiFi\",\"Паркинг\",\"СПА\",\"Фитнес\",\"Ресторант\",\"Рум Сервиз\",\"Конферентни зали\",\"Басейн\"]",
                CancellationPolicies = "[{\"daysBeforeCheckIn\":7,\"refundPercentage\":100},{\"daysBeforeCheckIn\":3,\"refundPercentage\":50},{\"daysBeforeCheckIn\":1,\"refundPercentage\":0}]",
                IsAvailable = true,
                CreatedById = adminId,
                CreatedAt = now.AddMonths(-6),
                UpdatedAt = now.AddDays(-5)
            },
            new Hotel
            {
                Name = "Морски Бриз Варна",
                Description = "Хотел с директен достъп до плажа в Морска градина. Предлага уютни стаи с морски изглед, открит басейн, водни спортове и вечеря на брега. Перфектен за семейна ваканция.",
                Location = "Морска градина, Варна 9000",
                City = "Варна",
                Country = "България",
                Latitude = 43.2046m,
                Longitude = 27.9280m,
                Rating = 4.5m,
                StarRating = 4,
                ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
                Images = "[\"https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800\",\"https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800\",\"https://images.unsplash.com/photo-1551882547-ff40c4a49f33?w=800\"]",
                Amenities = "[\"WiFi\",\"Плаж\",\"Басейн\",\"Водни спортове\",\"Ресторант\",\"Бар\",\"Анимация\",\"Паркинг\"]",
                CancellationPolicies = "[{\"daysBeforeCheckIn\":14,\"refundPercentage\":100},{\"daysBeforeCheckIn\":7,\"refundPercentage\":50},{\"daysBeforeCheckIn\":3,\"refundPercentage\":0}]",
                IsAvailable = true,
                CreatedById = adminId,
                CreatedAt = now.AddMonths(-5),
                UpdatedAt = now.AddDays(-10)
            },
            new Hotel
            {
                Name = "Планинска Идилия Банско",
                Description = "Уютен планински хотел в центъра на Банско. Ски влекове на 200 метра, ски склад, уелнес зона с джакузи и сауна. Традиционна кухня в механата и топла атмосфера.",
                Location = "ул. Никола Вапцаров 5, Банско 2770",
                City = "Банско",
                Country = "България",
                Latitude = 41.8302m,
                Longitude = 23.4877m,
                Rating = 4.7m,
                StarRating = 4,
                ImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
                Images = "[\"https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=800\",\"https://images.unsplash.com/photo-1585208798174-6cedd86e019a?w=800\",\"https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800\"]",
                Amenities = "[\"WiFi\",\"Ски Склад\",\"Сауна\",\"Джакузи\",\"Механа\",\"Паркинг\",\"Трансфер до Ски Влек\",\"СПА\"]",
                CancellationPolicies = "[{\"daysBeforeCheckIn\":10,\"refundPercentage\":100},{\"daysBeforeCheckIn\":5,\"refundPercentage\":50},{\"daysBeforeCheckIn\":2,\"refundPercentage\":0}]",
                IsAvailable = true,
                CreatedById = adminId,
                CreatedAt = now.AddMonths(-4),
                UpdatedAt = now.AddDays(-3)
            },
            new Hotel
            {
                Name = "Термален Рай Велинград",
                Description = "Луксозен СПА хотел в Балнеологичната столица на Балканите. Минерални басейни, лечебни процедури, масажи и пълен wellness пакет. Идеален за здравен туризъм и релакс.",
                Location = "ул. Съединение 40, Велинград 4600",
                City = "Велинград",
                Country = "България",
                Latitude = 42.0225m,
                Longitude = 23.9931m,
                Rating = 4.6m,
                StarRating = 4,
                ImageUrl = "https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800",
                Images = "[\"https://images.unsplash.com/photo-1515377905703-c4788e51af15?w=800\",\"https://images.unsplash.com/photo-1612468289577-1a7b0a8ee5C0?w=800\",\"https://images.unsplash.com/photo-1602002418082-a4443e081dd1?w=800\"]",
                Amenities = "[\"WiFi\",\"Минерален Басейн\",\"СПА\",\"Масажи\",\"Лечебни Процедури\",\"Ресторант\",\"Паркинг\",\"Фитнес\"]",
                CancellationPolicies = "[{\"daysBeforeCheckIn\":7,\"refundPercentage\":100},{\"daysBeforeCheckIn\":3,\"refundPercentage\":50}]",
                IsAvailable = true,
                CreatedById = adminId,
                CreatedAt = now.AddMonths(-3),
                UpdatedAt = now.AddDays(-7)
            },
            new Hotel
            {
                Name = "Бутик Хотел Пловдив",
                Description = "Очарователен бутик хотел в историческия Стария Град на Пловдив. Реставрирана възрожденска сграда, ресторант с традиционна кухня и тераса с гледка към Джумая джамия.",
                Location = "ул. Кирил Нektariev 3, Стар Град, Пловдив 4000",
                City = "Пловдив",
                Country = "България",
                Latitude = 42.1453m,
                Longitude = 24.7493m,
                Rating = 4.4m,
                StarRating = 3,
                ImageUrl = "https://images.unsplash.com/photo-1551016750-97cb93fa46c0?w=800",
                Images = "[\"https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800\",\"https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?w=800\"]",
                Amenities = "[\"WiFi\",\"Ресторант\",\"Тераса\",\"Закуска Включена\",\"Туристически Обиколки\"]",
                CancellationPolicies = "[{\"daysBeforeCheckIn\":5,\"refundPercentage\":100},{\"daysBeforeCheckIn\":2,\"refundPercentage\":0}]",
                IsAvailable = true,
                CreatedById = adminId,
                CreatedAt = now.AddMonths(-2),
                UpdatedAt = now.AddDays(-1)
            }
        };
    }

    // -----------------------------------------------------------------------
    // 3. Room Types
    // -----------------------------------------------------------------------
    private static List<RoomType> SeedRoomTypes(List<Hotel> hotels)
    {
        var now = DateTime.UtcNow;
        var roomTypes = new List<RoomType>();

        // Hotel 1 - Sofia (5★, premium pricing)
        roomTypes.AddRange(new[]
        {
            new RoomType { Hotel = hotels[0], Name = "Стандартна стая", Description = "Елегантна стая с кралско легло, луксозно спално бельо и баня с душ-кабина.", PricePerNight = 220m, Capacity = 2, TotalRooms = 15, ImageUrl = "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=600", CreatedAt = now, UpdatedAt = now },
            new RoomType { Hotel = hotels[0], Name = "Делукс стая", Description = "Просторна стая с изглед към Витоша, мини бар и сейф.", PricePerNight = 320m, Capacity = 2, TotalRooms = 10, ImageUrl = "https://images.unsplash.com/photo-1618773928121-c32242e63f39?w=600", CreatedAt = now, UpdatedAt = now },
            new RoomType { Hotel = hotels[0], Name = "Апартамент", Description = "Луксозен апартамент с хол, трапезария, кухненски бокс и две бани.", PricePerNight = 580m, Capacity = 4, TotalRooms = 5, ImageUrl = "https://images.unsplash.com/photo-1591088398332-8a7791972843?w=600", CreatedAt = now, UpdatedAt = now }
        });

        // Hotel 2 - Varna (4★, sea resort)
        roomTypes.AddRange(new[]
        {
            new RoomType { Hotel = hotels[1], Name = "Стая с морски изглед", Description = "Светла стая с балкон и директна гледка към Черно море.", PricePerNight = 180m, Capacity = 2, TotalRooms = 20, ImageUrl = "https://images.unsplash.com/photo-1540518614846-7eded433c457?w=600", CreatedAt = now, UpdatedAt = now },
            new RoomType { Hotel = hotels[1], Name = "Семейна стая", Description = "Просторна стая с две спални, подходяща за семейства с деца.", PricePerNight = 260m, Capacity = 4, TotalRooms = 10, ImageUrl = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600", CreatedAt = now, UpdatedAt = now },
            new RoomType { Hotel = hotels[1], Name = "Студио", Description = "Компактно студио с kitchenette, идеално за по-дълъг престой.", PricePerNight = 150m, Capacity = 2, TotalRooms = 8, ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=600", CreatedAt = now, UpdatedAt = now }
        });

        // Hotel 3 - Bansko (4★, ski resort)
        roomTypes.AddRange(new[]
        {
            new RoomType { Hotel = hotels[2], Name = "Стандартна стая", Description = "Уютна стая с дървени акценти, идеална след ски ден.", PricePerNight = 140m, Capacity = 2, TotalRooms = 18, ImageUrl = "https://images.unsplash.com/photo-1600210491369-e753d80a41f3?w=600", CreatedAt = now, UpdatedAt = now },
            new RoomType { Hotel = hotels[2], Name = "Апартамент с камина", Description = "Романтичен апартамент с работеща камина и джакузи.", PricePerNight = 320m, Capacity = 3, TotalRooms = 6, ImageUrl = "https://images.unsplash.com/photo-1600121848594-d8644e57abab?w=600", CreatedAt = now, UpdatedAt = now }
        });

        // Hotel 4 - Velingrad (4★, spa)
        roomTypes.AddRange(new[]
        {
            new RoomType { Hotel = hotels[3], Name = "СПА стая", Description = "Стая с директен достъп до минералния басейн и романтичен балкон.", PricePerNight = 200m, Capacity = 2, TotalRooms = 12, ImageUrl = "https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?w=600", CreatedAt = now, UpdatedAt = now },
            new RoomType { Hotel = hotels[3], Name = "Wellness Suite", Description = "Луксозен апартамент с частна сауна, джакузи и минерален душ.", PricePerNight = 400m, Capacity = 2, TotalRooms = 4, ImageUrl = "https://images.unsplash.com/photo-1631049552057-403cdb8f0658?w=600", CreatedAt = now, UpdatedAt = now }
        });

        // Hotel 5 - Plovdiv (3★, boutique)
        roomTypes.AddRange(new[]
        {
            new RoomType { Hotel = hotels[4], Name = "Исторически шарм", Description = "Автентична стая с оригинални дървени тавани и антикварна мебел.", PricePerNight = 110m, Capacity = 2, TotalRooms = 8, ImageUrl = "https://images.unsplash.com/photo-1595576508898-0ad5c879a061?w=600", CreatedAt = now, UpdatedAt = now },
            new RoomType { Hotel = hotels[4], Name = "Двойна с тераса", Description = "Стая с широка тераса и гледка към Стария Град.", PricePerNight = 145m, Capacity = 2, TotalRooms = 5, ImageUrl = "https://images.unsplash.com/photo-1560185127-6c35ac7e67fd?w=600", CreatedAt = now, UpdatedAt = now }
        });

        return roomTypes;
    }

    // -----------------------------------------------------------------------
    // 4. Room Availability (60 days from today, sufficient rooms each day)
    // -----------------------------------------------------------------------
    private static List<RoomAvailability> SeedRoomAvailability(List<RoomType> roomTypes)
    {
        var availabilities = new List<RoomAvailability>();
        var today = DateTime.UtcNow.Date;
        var now = DateTime.UtcNow;

        foreach (var rt in roomTypes)
        {
            for (int i = 0; i < 60; i++)
            {
                var date = today.AddDays(i);
                // Weekend slightly fewer rooms available
                var available = (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                    ? Math.Max(1, rt.TotalRooms - 3)
                    : rt.TotalRooms;

                availabilities.Add(new RoomAvailability
                {
                    RoomType = rt,
                    Date = date,
                    AvailableCount = available,
                    IsBlocked = false,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }

        return availabilities;
    }

    // -----------------------------------------------------------------------
    // 5. Room Discounts
    // -----------------------------------------------------------------------
    private static List<RoomDiscount> SeedRoomDiscounts(List<RoomType> roomTypes)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Early-bird summer discount on Varna sea-view room (index 3)
        // Weekend ski discount on Bansko standard room (index 5)
        return new List<RoomDiscount>
        {
            new RoomDiscount
            {
                RoomType = roomTypes[3], // Varna - Sea view
                StartDate = today.AddDays(30),
                EndDate = today.AddDays(60),
                DiscountPercentage = 15,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RoomDiscount
            {
                RoomType = roomTypes[5], // Bansko - Standard
                StartDate = today.AddDays(7),
                EndDate = today.AddDays(21),
                DiscountPercentage = 20,
                CreatedAt = now,
                UpdatedAt = now
            },
            new RoomDiscount
            {
                RoomType = roomTypes[0], // Sofia - Standard
                StartDate = today.AddDays(5),
                EndDate = today.AddDays(15),
                DiscountPercentage = 10,
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }

    // -----------------------------------------------------------------------
    // 6. Reservations (mix of Confirmed, Pending, Cancelled, Completed)
    // -----------------------------------------------------------------------
    private static List<Reservation> SeedReservations(
        List<ApplicationUser> users,
        List<Hotel> hotels,
        List<RoomType> roomTypes)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        return new List<Reservation>
        {
            // Completed - past stay in Sofia
            new Reservation
            {
                User = users[0], Hotel = hotels[0], RoomType = roomTypes[0],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(-30), CheckOutDate = today.AddDays(-25),
                TotalPrice = 1100m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Completed", Notes = "Специален случай - годишнина",
                CreatedAt = now.AddDays(-40), UpdatedAt = now.AddDays(-25)
            },
            // Confirmed - upcoming Sofia
            new Reservation
            {
                User = users[1], Hotel = hotels[0], RoomType = roomTypes[1],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(10), CheckOutDate = today.AddDays(13),
                TotalPrice = 960m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Confirmed", Notes = null,
                CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5)
            },
            // Completed - Varna summer
            new Reservation
            {
                User = users[0], Hotel = hotels[1], RoomType = roomTypes[3],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(-90), CheckOutDate = today.AddDays(-83),
                TotalPrice = 1260m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Completed", Notes = null,
                CreatedAt = now.AddDays(-100), UpdatedAt = now.AddDays(-83)
            },
            // Pending - Varna family
            new Reservation
            {
                User = users[2], Hotel = hotels[1], RoomType = roomTypes[4],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(20), CheckOutDate = today.AddDays(27),
                TotalPrice = 1820m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Pending", Notes = "Нужна е детска кошара",
                CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2)
            },
            // Confirmed - Bansko ski season
            new Reservation
            {
                User = users[3], Hotel = hotels[2], RoomType = roomTypes[5],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(15), CheckOutDate = today.AddDays(18),
                TotalPrice = 420m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Confirmed", Notes = null,
                CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-7)
            },
            // Cancelled - Bansko apartment
            new Reservation
            {
                User = users[4], Hotel = hotels[2], RoomType = roomTypes[6],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(-10), CheckOutDate = today.AddDays(-7),
                TotalPrice = 960m, RefundAmount = 480m, RetainedAmount = 480m,
                Status = "Cancelled", Notes = "Отменено поради болест",
                CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-12)
            },
            // Completed - Velingrad SPA
            new Reservation
            {
                User = users[1], Hotel = hotels[3], RoomType = roomTypes[7],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(-15), CheckOutDate = today.AddDays(-12),
                TotalPrice = 600m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Completed", Notes = null,
                CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-12)
            },
            // Confirmed - Plovdiv boutique
            new Reservation
            {
                User = users[2], Hotel = hotels[4], RoomType = roomTypes[9],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(5), CheckOutDate = today.AddDays(8),
                TotalPrice = 435m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Confirmed", Notes = "Искаме изглед към двора",
                CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3)
            },
            // Completed - Sofia apartment
            new Reservation
            {
                User = users[4], Hotel = hotels[0], RoomType = roomTypes[2],
                NumberOfRooms = 1,
                CheckInDate = today.AddDays(-60), CheckOutDate = today.AddDays(-57),
                TotalPrice = 1740m, RefundAmount = 0m, RetainedAmount = 0m,
                Status = "Completed", Notes = null,
                CreatedAt = now.AddDays(-70), UpdatedAt = now.AddDays(-57)
            }
        };
    }

    // -----------------------------------------------------------------------
    // 7. Hotel Reviews
    // -----------------------------------------------------------------------
    private static List<HotelReview> SeedHotelReviews(List<ApplicationUser> users, List<Hotel> hotels)
    {
        var now = DateTime.UtcNow;

        return new List<HotelReview>
        {
            new HotelReview { User = users[0], Hotel = hotels[0], Rating = 5, ReviewText = "Изключително обслужване и невероятна гледка! Ще се върнем задължително.", CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-25) },
            new HotelReview { User = users[1], Hotel = hotels[0], Rating = 5, ReviewText = "Перфектен хотел за бизнес пътуване. Конферентните зали са отлично оборудвани.", CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-10) },
            new HotelReview { User = users[2], Hotel = hotels[0], Rating = 4, ReviewText = "Много добър хотел, но цените са малко високи. Въпреки това качеството оправдава.", CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5) },

            new HotelReview { User = users[0], Hotel = hotels[1], Rating = 5, ReviewText = "Директен плаж, чудесен басейн, прекрасна анимация! Децата са напълно доволни!", CreatedAt = now.AddDays(-85), UpdatedAt = now.AddDays(-85) },
            new HotelReview { User = users[3], Hotel = hotels[1], Rating = 4, ReviewText = "Хубав хотел с добра локация. Храната в ресторанта е отлична.", CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-30) },

            new HotelReview { User = users[3], Hotel = hotels[2], Rating = 5, ReviewText = "Страхотна атмосфера, камината беше незабравима! Ски пистите са много близо.", CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-7) },
            new HotelReview { User = users[4], Hotel = hotels[2], Rating = 4, ReviewText = "Добър хотел за ски ваканция. Механата е автентична с вкусна традиционна кухня.", CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12) },

            new HotelReview { User = users[1], Hotel = hotels[3], Rating = 5, ReviewText = "Минералните басейни са изкушение! Масажите са топ. Препоръчвам го горещо.", CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12) },
            new HotelReview { User = users[0], Hotel = hotels[3], Rating = 4, ReviewText = "Отлично за почивка и лечение. Персоналът е много внимателен.", CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-20) },

            new HotelReview { User = users[2], Hotel = hotels[4], Rating = 4, ReviewText = "Прекрасна локация в Стария Град. Атмосферата е уникална.", CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3) },
            new HotelReview { User = users[4], Hotel = hotels[4], Rating = 5, ReviewText = "Истинска скъпоценност! Автентичен характер и топло посрещане.", CreatedAt = now.AddDays(-15), UpdatedAt = now.AddDays(-15) }
        };
    }

    // -----------------------------------------------------------------------
    // 8. Comments (including one reply thread per hotel)
    // -----------------------------------------------------------------------
    private static List<Comment> SeedComments(List<ApplicationUser> users, List<Hotel> hotels)
    {
        var now = DateTime.UtcNow;
        var topLevelComments = new List<Comment>
        {
            // Hotel 1 - Sofia
            new Comment { Hotel = hotels[0], User = users[0], Text = "Хотелът е просто великолепен! Стаята беше чиста и уютна, а персоналът - изключително учтив.", CreatedAt = now.AddDays(-26), UpdatedAt = now.AddDays(-26) },
            new Comment { Hotel = hotels[0], User = users[2], Text = "Много добра локация в центъра. Ресторантът предлага отлична храна с добри порции.", CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12) },
            new Comment { Hotel = hotels[0], User = users[4], Text = "Безупречно обслужване от момента на пристигане до заминаване. Определено ще се върна!", CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-8) },

            // Hotel 2 - Varna
            new Comment { Hotel = hotels[1], User = users[0], Text = "Морският бриз наистина оправдава името си! Плажът е на 50 метра и е много чист.", CreatedAt = now.AddDays(-86), UpdatedAt = now.AddDays(-86) },
            new Comment { Hotel = hotels[1], User = users[3], Text = "Чудесен семеен хотел. Анимацията за деца е много добре организирана.", CreatedAt = now.AddDays(-32), UpdatedAt = now.AddDays(-32) },

            // Hotel 3 - Bansko
            new Comment { Hotel = hotels[2], User = users[3], Text = "Перфектна ски ваканция! Влековете са на пешеходно разстояние, а механата е топла и уютна.", CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-8) },
            new Comment { Hotel = hotels[2], User = users[1], Text = "Камината в апартамента беше истинска романтика. Определено топ избор за двойки!", CreatedAt = now.AddDays(-14), UpdatedAt = now.AddDays(-14) },

            // Hotel 4 - Velingrad
            new Comment { Hotel = hotels[3], User = users[1], Text = "Минералните басейни са фантастични. Водата е перфектна температура и е много релаксираща!", CreatedAt = now.AddDays(-13), UpdatedAt = now.AddDays(-13) },
            new Comment { Hotel = hotels[3], User = users[4], Text = "Идеалното място за детоксикация от ежедневния стрес. Масажите са великолепни!", CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5) },

            // Hotel 5 - Plovdiv
            new Comment { Hotel = hotels[4], User = users[2], Text = "Невероятна автентична атмосфера! Сградата е реставрирана с огромно внимание към детайла.", CreatedAt = now.AddDays(-4), UpdatedAt = now.AddDays(-4) },
            new Comment { Hotel = hotels[4], User = users[0], Text = "Локацията в Стария Град е перфектна за разходки и туризъм. Закуската е вкусна!", CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2) }
        };

        // Replies  
        var replies = new List<Comment>
        {
            new Comment { Hotel = hotels[0], User = users[1], Text = "Напълно съгласен! Аз също останах много доволен от персонала.", ParentComment = topLevelComments[0], CreatedAt = topLevelComments[0].CreatedAt.AddHours(3), UpdatedAt = topLevelComments[0].CreatedAt.AddHours(3) },
            new Comment { Hotel = hotels[1], User = users[2], Text = "Съгласна съм за плажа! Феноменално място за деца.", ParentComment = topLevelComments[3], CreatedAt = topLevelComments[3].CreatedAt.AddHours(5), UpdatedAt = topLevelComments[3].CreatedAt.AddHours(5) },
            new Comment { Hotel = hotels[2], User = users[2], Text = "И ние много харесахме механата! Баницата беше домашна.", ParentComment = topLevelComments[5], CreatedAt = topLevelComments[5].CreatedAt.AddHours(2), UpdatedAt = topLevelComments[5].CreatedAt.AddHours(2) }
        };

        var all = new List<Comment>();
        all.AddRange(topLevelComments);
        all.AddRange(replies);
        return all;
    }

    // -----------------------------------------------------------------------
    // 9. Comment Ratings
    // -----------------------------------------------------------------------
    private static List<CommentRating> SeedCommentRatings(List<ApplicationUser> users, List<Comment> comments)
    {
        var now = DateTime.UtcNow;
        // Rate only top-level comments (those without a parent)
        var topLevel = comments.Where(c => c.ParentCommentId == null && c.ParentComment == null).ToList();
        var ratings = new List<CommentRating>();

        // Each user rates a few comments they didn't write
        for (int i = 0; i < topLevel.Count; i++)
        {
            var comment = topLevel[i];
            // Two other users like each comment
            foreach (var user in users.Where(u => u.Id != comment.UserId).Take(2))
            {
                ratings.Add(new CommentRating
                {
                    Comment = comment,
                    User = user,
                    IsLike = true,
                    CreatedAt = comment.CreatedAt.AddHours(1)
                });
            }
        }

        return ratings;
    }

    // -----------------------------------------------------------------------
    // 10. Favorites
    // -----------------------------------------------------------------------
    private static List<Favorite> SeedFavorites(List<ApplicationUser> users, List<Hotel> hotels)
    {
        var now = DateTime.UtcNow;

        return new List<Favorite>
        {
            new Favorite { User = users[0], Hotel = hotels[0], CreatedAt = now.AddDays(-50) },
            new Favorite { User = users[0], Hotel = hotels[1], CreatedAt = now.AddDays(-45) },
            new Favorite { User = users[0], Hotel = hotels[2], CreatedAt = now.AddDays(-20) },
            new Favorite { User = users[1], Hotel = hotels[0], CreatedAt = now.AddDays(-30) },
            new Favorite { User = users[1], Hotel = hotels[3], CreatedAt = now.AddDays(-10) },
            new Favorite { User = users[2], Hotel = hotels[1], CreatedAt = now.AddDays(-25) },
            new Favorite { User = users[2], Hotel = hotels[4], CreatedAt = now.AddDays(-5) },
            new Favorite { User = users[3], Hotel = hotels[2], CreatedAt = now.AddDays(-15) },
            new Favorite { User = users[3], Hotel = hotels[3], CreatedAt = now.AddDays(-8) },
            new Favorite { User = users[4], Hotel = hotels[0], CreatedAt = now.AddDays(-40) },
            new Favorite { User = users[4], Hotel = hotels[4], CreatedAt = now.AddDays(-3) }
        };
    }

    // -----------------------------------------------------------------------
    // 11. Promo Codes
    // -----------------------------------------------------------------------
    private static List<PromoCode> SeedPromoCodes(List<Hotel> hotels)
    {
        var now = DateTime.UtcNow;

        return new List<PromoCode>
        {
            // Global promo codes (HotelId = null)
            new PromoCode { Code = "WELCOME10", DiscountPercentage = 10m, MaxActivations = 100, CurrentActivations = 12, ExpirationDate = now.AddDays(60), CreatedAt = now.AddDays(-30) },
            new PromoCode { Code = "SUMMER25", DiscountPercentage = 25m, MaxActivations = 50, CurrentActivations = 8, ExpirationDate = now.AddDays(90), CreatedAt = now.AddDays(-10) },

            // Hotel-specific promo codes
            new PromoCode { Hotel = hotels[0], Code = "SOFIA15", DiscountPercentage = 15m, MaxActivations = 30, CurrentActivations = 5, ExpirationDate = now.AddDays(45), CreatedAt = now.AddDays(-20) },
            new PromoCode { Hotel = hotels[1], Code = "BEACH20", DiscountPercentage = 20m, MaxActivations = 40, CurrentActivations = 15, ExpirationDate = now.AddDays(120), CreatedAt = now.AddDays(-15) },
            new PromoCode { Hotel = hotels[2], Code = "SKI30", DiscountPercentage = 30m, MaxActivations = 25, CurrentActivations = 3, ExpirationDate = now.AddDays(30), CreatedAt = now.AddDays(-5) },
            new PromoCode { Hotel = hotels[3], Code = "SPA15", DiscountPercentage = 15m, MaxActivations = 20, CurrentActivations = 7, ExpirationDate = now.AddDays(60), CreatedAt = now.AddDays(-12) }
        };
    }
}
