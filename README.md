# Petoria - Premier Hotel Booking Platform

Petoria is a state-of-the-art hotel reservation system built with .NET 8 and React 18. It offers a seamless experience for guests to discover and book accomodations, while providing hotel managers and administrators with powerful tools to manage properties, rooms, and bookings.

## 🌟 Key Features

### 🏨 For Guests
*   **Smart Search & Filtering**: Filter hotels by real-time availability, price range, city, amenities, and star rating.
*   **Interactive Map**: Explore hotels geographically using an integrated Leaflet map with clustering.
*   **Detailed Hotel Views**: View high-quality image carousels, comprehensive descriptions, and amenity lists.
*   **Review System**: Read and write verified reviews with ratings.
*   **Comment Discussions**: Engage with the community and hotel owners via a nested comment system.
*   **Favorites**: Save hotels to a personal favorites list for quick access.
*   **Booking Management**: Easy booking process with instant confirmation and purchase history.
*   **Deals & Offers**: Exclusive access to seasonal discounts, last-minute deals, and packages.
*   **Multilingual Support**: Fully localized interface in **English** and **Bulgarian**.
*   **Support Center**: Direct communication channel with hotel administration and platform support.

### 💼 For Hotel Managers
*   **Property Management**: Create and edit hotel listings with location data and photos.
*   **Room Management**: Define custom room types, pricing, and capacity (`RoomTypeManager`).
*   **Availability Control**: Visual calendar (`AvailabilityCalendar`) for managing block dates and bulk updates.
*   **Discount & Pricing Strategy**: Create dynamic discount campaigns (`DiscountManager`) to attract more guests.
*   **Guest Communication**: Respond to guest messages and inquiries directly.
*   **Performance Tracking**: View dashboard statistics on bookings and revenue.

### 🛡️ For Administrators (SuperAdmin)
*   **Platform Oversight**: Global dashboard (`AdminDashboard`) to monitor all users, hotels, and system activity.
*   **User Management**: Manage user roles and permissions.
*   **Content Moderation**: Oversee reviews and comments.
*   **System-wide Support**: Handle high-level support tickets (`AdminSupportMessages`).

## 🛠️ Technology Stack

### Frontend
*   **Core**: React 18, Vite
*   **State Management**: React Context API (`AuthContext`, `FavoritesContext`, `LanguageContext`)
*   **Routing**: React Router DOM v6
*   **HTTP Client**: Native `fetch` with a custom `api.js` wrapper (Automatic cookie handling)
*   **Maps**: Leaflet, React Leaflet
*   **Styling**: CSS Modules for scoped styling
*   **Localization**: i18n support

### Backend
*   **Framework**: ASP.NET Core 8 Web API
*   **Database**: SQL Server (Entity Framework Core)
*   **Authentication**: ASP.NET Core Identity with **HTTP-Only Cookies** (Secure, XSS-resistant)
*   **Architecture**: Layered architecture (Controllers, Services, Repositories, DTOs)

## 📂 Project Structure

```
Petoria/
├── backend/
│   ├── Petoria/                # Main Web API project
│   ├── Petoria.Data/           # EF Core DbContext and Migrations
│   ├── Petoria.Models/         # Domain entities and DTOs
│   └── Petoria.Services/       # Business logic layer
└── frontend/                   # React Application
    ├── src/
    │   ├── assets/             # Images and global styles
    │   ├── components/         # Reusable UI components
    │   │   ├── BookingWidget   # Reservation logic
    │   │   ├── HotelMap        # Map integrations
    │   │   └── ...
    │   ├── context/            # Global state providers
    │   ├── pages/              # Application routes
    │   │   ├── AdminDashboard  # Admin functionalities
    │   │   ├── ManageHotel     # Hotel owner tools
    │   │   └── ...
    │   ├── utils/              # Helper functions (api.js, validators)
    │   └── locales/            # Translation files
    └── public/
```

## 🚀 Getting Started

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/) (v16+)
*   [SQL Server](https://www.microsoft.com/sql-server/)

### Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/Magesnik/Petoria.git
    cd Petoria
    ```

2.  **Database Setup:**
    Configure your connection string in `backend/Petoria/appsettings.json`.
    ```bash
    cd backend/Petoria
    dotnet restore
    dotnet ef database update
    ```

3.  **Run Backend:**
    ```bash
    dotnet run
    # API will start at http://localhost:5150
    ```

4.  **Run Frontend:**
    ```bash
    cd ../../frontend
    npm install
    npm run dev
    # UI will start at http://localhost:5173
    ```

## 🔐 Security & Authentication

Petoria uses a secure, modern authentication flow:
*   **HttpOnly Cookies**: JWT tokens are stored in HttpOnly cookies, preventing access from client-side JavaScript.
*   **CSRF Protection**: Native browser protections combined with safe HTTP methods.
*   **Role-Based Access Control (RBAC)**: Strict authorization policies for `Admin`, `HotelOwner`, and `User` roles.

## 👥 Team

*   **Georgi Magesnik** - Full Stack Developer
*   **Nikolay Stoyanov** - Full Stack Developer

## 📄 License

This project is licensed under the MIT License.