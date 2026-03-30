# 🏨 Petoria — Premier Hotel Booking Platform

**Petoria** is a full-stack hotel reservation system built with **ASP.NET Core 9** and **React 19**. It offers a seamless experience for guests to discover and book accommodations, while giving hotel owners and administrators powerful tools to manage properties, rooms, bookings, pricing, and communications.

---

## 🌟 Key Features

### 👤 For Guests
- **Smart Search & Filtering** — Filter hotels by availability, price range, city, amenities, and star rating.
- **Interactive Map** — Explore hotels geographically via an integrated Leaflet map with marker clustering.
- **Detailed Hotel Views** — High-quality image carousels, descriptions, amenity lists, and room types.
- **Review System** — Read and post verified reviews with star ratings.
- **Comment Discussions** — Nested comment threads with community interaction.
- **Favorites** — Save hotels to a personal favourites list.
- **Shopping Cart & Checkout** — Add rooms to cart and pay securely via **Stripe**.
- **Purchase History** — Review all past and upcoming reservations.
- **Deals & Packages** — Access seasonal discounts, last-minute offers, and promo codes.
- **Promo Codes** — Apply discount codes at checkout.
- **Multilingual UI** — Fully localised interface in **English** 🇬🇧 and **Bulgarian** 🇧🇬.
- **Multi-Currency Support** — View prices in multiple currencies.
- **Google OAuth Login** — Sign in with a Google account in one click.
- **Support Center** — Send support messages directly from the platform.
- **Hotel Contact** — Message hotel management directly.

### 🏢 For Hotel Owners
- **Property Management** — Create and edit hotel listings with location, photos (via Cloudinary), and descriptions.
- **Room Type Manager** — Define custom room types, pricing, capacity, and amenities.
- **Availability Calendar** — Visual calendar for blocking dates, viewing occupancy, and bulk availability updates.
- **Discount Manager** — Create time-limited discount campaigns per room type.
- **Promo Code Manager** — Issue and manage hotel-specific promo codes.
- **Guest Communication** — Manage incoming guest messages and inquiries.
- **Email Notifications** — Automated confirmation and cancellation emails sent to guests via SMTP (Gmail/MailKit).
- **Revenue Dashboard** — Booking and revenue statistics.

### 🛡️ For Administrators
- **Admin Dashboard** — Global overview of users, hotels, revenue, and activity.
- **User & Role Management** — Assign and manage roles (`Admin`, `HotelOwner`, `User`, `Moderator`).
- **Global Promo Codes** — Issue platform-wide discount codes.
- **Content Moderation** — Oversee reviews and comments across all hotels.
- **Support Ticket Handling** — Manage and respond to high-level support messages.
- **Moderator Assignment** — Assign moderators to specific hotels.

---

## 🛠️ Technology Stack

### Frontend
| Technology | Purpose |
|---|---|
| React 19 + Vite | UI framework & build tool |
| React Router DOM v7 | Client-side routing |
| React Context API | Global state (Auth, Language, Currency, Favorites, Cart, Theme) |
| Leaflet + React Leaflet | Interactive hotel map |
| `@react-oauth/google` | Google OAuth login |
| `jwt-decode` | Client-side JWT inspection |
| CSS Modules (plain CSS) | Scoped component styling |
| i18n (custom) | Bilingual EN/BG localisation |

### Backend
| Technology | Purpose |
|---|---|
| ASP.NET Core 9 Web API | REST API framework |
| Entity Framework Core | ORM & database migrations |
| SQL Server | Relational database |
| ASP.NET Core Identity | User management & password hashing |
| JWT (HTTP-Only Cookies) | Stateless authentication, XSS-resistant |
| Stripe | Online payment processing |
| Cloudinary | Cloud image storage & delivery |
| MailKit / SMTP | Transactional email notifications |
| Swagger / OpenAPI | Auto-generated API documentation |

---

## 📂 Project Structure

```
Petoria/
├── backend/
│   ├── Petoria/                        # Main Web API project (entry point)
│   │   ├── Controllers/                # 18 API controllers
│   │   │   ├── AdminController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── AvailabilityController.cs
│   │   │   ├── CartController.cs
│   │   │   ├── CommentsController.cs
│   │   │   ├── DealsController.cs
│   │   │   ├── DiscountsController.cs
│   │   │   ├── FavoritesController.cs
│   │   │   ├── HotelMessagesController.cs
│   │   │   ├── HotelsController.cs
│   │   │   ├── ProfileController.cs
│   │   │   ├── PromoCodesController.cs
│   │   │   ├── ReservationsController.cs
│   │   │   ├── ReviewsController.cs
│   │   │   ├── RoomsController.cs
│   │   │   ├── StripeController.cs
│   │   │   ├── SupportMessagesController.cs
│   │   │   └── UploadController.cs
│   │   ├── Program.cs                  # DI, middleware, seeding
│   │   └── appsettings.json            # Configuration (DB, JWT, Stripe, Cloudinary, SMTP)
│   │
│   ├── Petoria.Core/                   # Business logic layer
│   │   ├── Contracts/                  # Service interfaces (IAuthService, IEmailService, IPhotoService)
│   │   ├── DTOs/                       # Data Transfer Objects (15 feature folders)
│   │   ├── Models/
│   │   │   ├── Auth/                   # Auth-related models
│   │   │   └── Email/                  # Email context & SMTP settings
│   │   ├── Services/                   # Implementations (AuthService, CloudinaryService, EmailService)
│   │   └── Utilities/
│   │       ├── EmailTemplateBuilder.cs # HTML email generator
│   │       └── EmailStyles.css         # Embedded email stylesheet
│   │
│   ├── Petoria.Infrastructure/         # Data access layer
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs # EF Core DbContext
│   │   │   ├── DatabaseSeeder.cs       # Demo data seeder
│   │   │   └── Entities/               # 15 domain entities
│   │   │       ├── ApplicationUser.cs
│   │   │       ├── Hotel.cs
│   │   │       ├── RoomType.cs
│   │   │       ├── RoomAvailability.cs
│   │   │       ├── RoomDiscount.cs
│   │   │       ├── Reservation.cs
│   │   │       ├── CartItem.cs
│   │   │       ├── Comment.cs
│   │   │       ├── CommentRating.cs
│   │   │       ├── HotelReview.cs
│   │   │       ├── Favorite.cs
│   │   │       ├── HotelMessage.cs
│   │   │       ├── HotelModerator.cs
│   │   │       ├── PromoCode.cs
│   │   │       └── SupportMessage.cs
│   │   └── Migrations/                 # EF Core database migrations
│   │
│   └── Petoria.Constants/              # Shared constants
│       ├── Roles.cs                    # Role name constants
│       └── ValidationConstants.cs      # Centralised validation limits
│
│   └── Petoria.Tests/                 # xUnit unit test project
│       ├── Helpers/
│       │   └── ControllerTestBase.cs  # Shared test base (InMemory DB, mocking)
│       ├── Controllers/               # 18 controller test files (145 tests)
│       ├── Utilities/
│       │   └── EmailTemplateBuilderTests.cs
│       └── Constants/
│           └── ValidationConstantsTests.cs
│
└── frontend/                           # React 19 application (Vite)
    ├── index.html
    ├── vite.config.js
    ├── package.json
    └── src/
        ├── main.jsx                    # App entry point
        ├── App.jsx                     # Root component & route definitions
        ├── index.css / App.css         # Global styles
        ├── assets/                     # Static images & icons
        ├── context/                    # React Context providers
        │   ├── AuthContext.jsx
        │   ├── CartContext.jsx
        │   ├── CurrencyContext.jsx
        │   ├── FavoritesContext.jsx
        │   ├── LanguageContext.jsx
        │   └── ThemeContext.jsx
        ├── components/                 # Reusable UI components
        │   ├── Header / Footer
        │   ├── BookingWidget
        │   ├── HotelCard / HotelMap / HotelFilters
        │   ├── SearchBar
        │   ├── AvailabilityCalendar / DateRangeCalendar
        │   ├── DiscountManager / RoomTypeManager / PromoCodeManager
        │   ├── ReviewSection
        │   ├── CommentsSection / Comment / CommentForm
        │   ├── AddToCartDialog
        │   └── LocationPicker
        ├── pages/
        │   ├── public/                 # Home, About, Deals, Support
        │   ├── auth/                   # Login, Register
        │   ├── hotels/                 # Hotels, HotelDetails, CreateHotel, ManageHotel,
        │   │                           # MyHotels, ContactHotel, HotelMessages
        │   ├── user/                   # Cart, Favorites, Settings, PurchaseHistory,
        │   │                           # MyMessages, PaymentSuccess, PaymentCancel
        │   ├── admin/                  # AdminDashboard, AdminSupportMessages, GlobalPromoCodes
        │   └── moderator/              # ModeratorDashboard, ModeratorHotelPanel
        ├── locales/
        │   ├── en.json                 # English translations
        │   └── bg.json                 # Bulgarian translations
        └── utils/
            └── api.js                  # Centralised fetch wrapper (auto cookie handling)
```

---

## 🔐 Security & Authentication

- **HttpOnly Cookies** — JWT tokens are stored in `HttpOnly` cookies, inaccessible from JavaScript (XSS-resistant).
- **JWT Validation** — Full issuer, audience, signing key, and lifetime validation on every request.
- **Role-Based Access Control (RBAC)** — Strict authorization with four roles:
  - `User` — Standard guests
  - `HotelOwner` — Property managers
  - `Moderator` — Hotel content moderators
  - `Admin` — Full platform access
- **Google OAuth** — Secure third-party sign-in via `@react-oauth/google`.
- **Stripe Webhooks** — Payment events processed server-side.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js v18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server/) (or SQL Server Express)

### 1. Clone the Repository
```bash
git clone https://github.com/Magesnik/Petoria.git
cd Petoria
```

### 2. Configure the Backend
Edit `backend/Petoria/appsettings.json` and fill in your values:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=PetoriaDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "PetoriaApi",
    "Audience": "PetoriaClient"
  },
  "Cloudinary": { "CloudName": "...", "ApiKey": "...", "ApiSecret": "..." },
  "Stripe": { "SecretKey": "sk_...", "PublishableKey": "pk_..." },
  "Smtp": { "Host": "smtp.gmail.com", "Port": 587, "Username": "...", "Password": "..." }
}
```

### 3. Apply Migrations & Run the Backend
```bash
cd backend/Petoria
dotnet restore
dotnet ef database update
dotnet run
# API available at: https://localhost:7150 / http://localhost:5150
```

> The application automatically seeds roles, an admin user, and demo data on first run.

### 4. Run the Frontend
```bash
cd ../../frontend
npm install
npm run dev
# UI available at: http://localhost:5173
```

---

## � API Overview

The REST API is organised around these resource endpoints:

| Controller | Route Prefix | Responsibility |
|---|---|---|
| `AuthController` | `/api/auth` | Login, register, Google OAuth, logout |
| `HotelsController` | `/api/hotels` | Hotel CRUD, search, filtering |
| `RoomsController` | `/api/rooms` | Room type management |
| `AvailabilityController` | `/api/availability` | Room availability & blocking |
| `ReservationsController` | `/api/reservations` | Booking lifecycle management |
| `CartController` | `/api/cart` | Shopping cart operations |
| `StripeController` | `/api/stripe` | Payment session & webhook |
| `DealsController` | `/api/deals` | Deals & featured offers |
| `DiscountsController` | `/api/discounts` | Room discount campaigns |
| `PromoCodesController` | `/api/promocodes` | Promo code creation & validation |
| `ReviewsController` | `/api/reviews` | Hotel reviews |
| `CommentsController` | `/api/comments` | Comment threads |
| `FavoritesController` | `/api/favorites` | User favourites |
| `ProfileController` | `/api/profile` | User profile & settings |
| `HotelMessagesController` | `/api/hotel-messages` | Hotel ↔ Guest messaging |
| `SupportMessagesController` | `/api/support-messages` | Platform support tickets |
| `AdminController` | `/api/admin` | Admin-level management |
| `UploadController` | `/api/upload` | Cloudinary image uploads |

Full OpenAPI docs are available at `https://localhost:7150/openapi/v1.json` in development.

---

## 🧪 Testing

### Unit Tests

The project includes a comprehensive **xUnit** test suite in `Petoria.Tests` with **153 tests** covering:

| Category | Tests | Coverage |
|---|---|---|
| Controller Tests | 145 | All 18 controllers — CRUD, auth, validation, error handling |
| Email Template Tests | 8 | Confirmation & cancellation emails in EN/BG |
| Validation Constants | 11 | Boundary value verification |

**Technologies used:** xUnit, Moq, EF Core InMemory provider.

```bash
cd backend
dotnet test Petoria.Tests --verbosity normal
```

### Cross-Browser & Device Testing

The application has been tested across multiple browsers and devices:

|     Browser   | Desktop |   Mobile  |
|    ---        |   ----  |    ---    |
| Chrome 120+   |    ✅   |    ✅    |
| Firefox 120+  |    ✅   |    ✅    |
| Safari 17+    |    ✅   | ✅ (iOS) |
| Edge 120+     |    ✅   |    ✅    |

The responsive layout uses CSS media queries and flexbox/grid to ensure proper display across screen sizes (320px–2560px).

---

## ⚡ Performance & Security Optimizations

### Performance
- **Response Compression** — Gzip/Brotli compression enabled via `ResponseCompression` middleware for smaller payloads.
- **EF Core query optimization** — Selective `Include()`, pagination, and `AsNoTracking()` for read-only queries.
- **Static file caching** — Frontend assets served with cache headers via `UseStaticFiles()`.
- **Lazy loading** — React components use `React.lazy()` and `Suspense` for code splitting.

### Security
- **HttpOnly JWT Cookies** — Tokens stored in HttpOnly cookies, immune to XSS.
- **Security Headers** — `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`, `Referrer-Policy`, and `Content-Security-Policy` headers added via middleware.
- **Rate Limiting** — ASP.NET Core rate limiting middleware to prevent brute-force and DDoS attacks.
- **Input Validation** — Centralised validation constants with data annotations on all DTOs.
- **CORS Restrictions** — Only whitelisted frontend origins allowed.
- **Role-Based Access Control** — `[Authorize(Roles = ...)]` on all sensitive endpoints.
- **Stripe Webhook Verification** — Server-side payment event validation.

---

## 📧 Email Notifications

Petoria sends transactional emails using **MailKit** over SMTP:
- ✅ **Reservation Confirmation** — Sent on successful booking with stay details, room info, and price breakdown.
- ❌ **Cancellation Notice** — Sent when a reservation is cancelled by the guest or owner.

Email templates are rendered from embedded HTML/CSS via `EmailTemplateBuilder` and support both EN and BG locale.

---