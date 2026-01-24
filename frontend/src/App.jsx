import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { ThemeProvider } from './context/ThemeContext';
import { LanguageProvider } from './context/LanguageContext';
import { AuthProvider } from './context/AuthContext';
import { FavoritesProvider } from './context/FavoritesContext';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Hotels from './pages/Hotels';
import HotelDetails from './pages/HotelDetails';
import CreateHotel from './pages/CreateHotel';
import Destinations from './pages/Destinations';
import About from './pages/About';
import Settings from './pages/Settings';
import Favorites from './pages/Favorites';
import PurchaseHistory from './pages/PurchaseHistory';
import Support from './pages/Support';
import AdminDashboard from './pages/AdminDashboard';
import MyHotels from './pages/MyHotels';
import ManageHotel from './pages/ManageHotel';

function App() {
  return (
    <GoogleOAuthProvider clientId="21847094498-c94136osjkahal0fjg0nk9q4mc7e4um9.apps.googleusercontent.com">
      <ThemeProvider>
        <LanguageProvider>
          <AuthProvider>
            <FavoritesProvider>
              <Router>
                <div className="App">
                  <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/hotels" element={<Hotels />} />
                    <Route path="/hotels/:id" element={<HotelDetails />} />
                    <Route path="/destinations" element={<Destinations />} />
                    <Route path="/about" element={<About />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/create-hotel" element={<CreateHotel />} />
                    <Route path="/settings" element={<Settings />} />
                    <Route path="/favorites" element={<Favorites />} />
                    <Route path="/purchase-history" element={<PurchaseHistory />} />
                    <Route path="/support" element={<Support />} />
                    <Route path="/admin" element={<AdminDashboard />} />
                    <Route path="/my-hotels" element={<MyHotels />} />
                    <Route path="/manage-hotel/:id" element={<ManageHotel />} />
                    <Route path="/hotel/:id" element={<HotelDetails />} />
                  </Routes>
                </div>
              </Router>
            </FavoritesProvider>
          </AuthProvider>
        </LanguageProvider>
      </ThemeProvider>
    </GoogleOAuthProvider>
  );
}

export default App;
