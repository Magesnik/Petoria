# Petoria - Online Hotel Booking Platform

Petoria is a modern, feature-rich online hotel reservation system built with .NET 8 and React. It allows hotel owners to easily manage their properties and guests to find and book the perfect accommodation.

## 🚀 Features

### For Guests
- **Advanced Search & Filtering**: Find hotels by location, price, rating, amenities, and availability.
- **Interactive Map**: View hotel locations on a map and explore nearby points of interest.
- **Real-time Availability**: Check room availability for specific dates.
- **Booking System**: Securely book rooms with instant confirmation.
- **User Accounts**: Manage profile, view booking history, and save favorite hotels.
- **Multi-language Support**: Switch between English and Bulgarian.
- **Reviews & Ratings**: Read and write reviews to share experiences.
- **Deals & Discounts**: Discover last-minute deals and special offers.

### For Hotel Owners
- **Hotel Management**: Create and manage hotel listings with detailed information.
- **Room Management**: Add and configure different room types.
- **Pricing & Discounts**: Set prices and create discount campaigns.
- **Booking Management**: View and manage all reservations.
- **Analytics Dashboard**: Track performance metrics and occupancy rates.
- **Amenity Management**: Customize hotel amenities with ease.

## 🛠️ Tech Stack

### Backend
- **Framework**: .NET 8
- **Language**: C#
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity
- **APIs**: RESTful APIs with JWT authentication

### Frontend
- **Framework**: React 18
- **Language**: JavaScript (ES6+)
- **Styling**: CSS Modules
- **Mapping**: Leaflet with React-Leaflet
- **State Management**: React Context API
- **HTTP Client**: Axios

## 📂 Project Structure

```
Petoria/
├── backend/                    # .NET 8 Backend API
│   ├── Petoria/                # Main API project
│   ├── Petoria.Data/           # Entity Framework Core
│   ├── Petoria.Models/         # Data models and DTOs
│   └── Petoria.Services/       # Business logic
└── frontend/                   # React Frontend Application
    ├── src/
    │   ├── components/         # Reusable UI components
    │   ├── context/            # React Context providers
    │   ├── pages/              # Page components
    │   ├── services/           # API service layer
    │   └── assets/             # Static assets
    └── public/                 # Public assets
```

## 🔌 API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/me` - Get current user profile

### Hotels
- `GET /api/hotels` - Get all hotels with optional filters
- `GET /api/hotels/{id}` - Get hotel details
- `POST /api/hotels` - Create a new hotel (admin)
- `PUT /api/hotels/{id}` - Update hotel (admin)
- `DELETE /api/hotels/{id}` - Delete hotel (admin)

### Bookings
- `GET /api/bookings` - Get user's bookings
- `POST /api/bookings` - Create a new booking
- `GET /api/bookings/admin` - Get all bookings (admin)
- `PUT /api/bookings/{id}/cancel` - Cancel a booking (admin)

### Reviews
- `GET /api/hotels/{hotelId}/reviews` - Get hotel reviews
- `POST /api/hotels/{hotelId}/reviews` - Add a review

### Favorites
- `GET /api/favorites` - Get user's favorite hotels
- `POST /api/favorites/{hotelId}` - Add hotel to favorites
- `DELETE /api/favorites/{hotelId}` - Remove hotel from favorites

## 🗺️ Map Integration

The application uses Leaflet for interactive maps. You can explore hotels by location and view nearby points of interest.

## 🌐 Multi-language Support

The system supports two languages:
- **English** (default)
- **Bulgarian** (bg)

Switch languages using the toggle in the header.

## 🔐 Authentication

Users can register and login to access personalized features. All API requests require authentication using JWT tokens.

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v16 or higher)
- [npm](https://www.npmjs.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Petoria
   ```

2. **Backend Setup**
   ```bash
   cd backend/Petoria
   dotnet restore
   dotnet ef database update
   dotnet run
   ```

3. **Frontend Setup**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

4. **Access the Application**
   Open [http://localhost:5174](http://localhost:5174) in your browser.

## 👥 Team

- **Georgi Magesnik** - Full Stack Developer
- **Nikolay Stoyanov** - Full Stack Developer

## 📄 License

This project is licensed under the MIT License.