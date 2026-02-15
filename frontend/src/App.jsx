import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { ThemeProvider } from './context/ThemeContext';
import { LanguageProvider } from './context/LanguageContext';
import { AuthProvider } from './context/AuthContext';
import { FavoritesProvider } from './context/FavoritesContext';
import { CurrencyProvider } from './context/CurrencyContext';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Hotels from './pages/Hotels';
import HotelDetails from './pages/HotelDetails';
import CreateHotel from './pages/CreateHotel';
import Deals from './pages/Deals';
import About from './pages/About';
import Settings from './pages/Settings';
import Favorites from './pages/Favorites';
import PurchaseHistory from './pages/PurchaseHistory';
import Support from './pages/Support';
import AdminDashboard from './pages/AdminDashboard';
import MyHotels from './pages/MyHotels';
import ManageHotel from './pages/ManageHotel';
import ContactHotel from './pages/ContactHotel';
import HotelMessages from './pages/HotelMessages';
import MyMessages from './pages/MyMessages';
import AdminSupportMessages from './pages/AdminSupportMessages';

function App() {
  return (
    <GoogleOAuthProvider clientId="21847094498-c94136osjkahal0fjg0nk9q4mc7e4um9.apps.googleusercontent.com">
      <ThemeProvider>
        <LanguageProvider>
          <AuthProvider>
            <FavoritesProvider>
              <CurrencyProvider>
                <Router>
                  <div className="App">
                    <Routes>
                      <Route path="/" element={<Home />} />
                      <Route path="/hotels" element={<Hotels />} />
                      <Route path="/hotels/:id" element={<HotelDetails />} />
                      <Route path="/deals" element={<Deals />} />
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
                      <Route path="/hotel/:id/contact" element={<ContactHotel />} />
                      <Route path="/hotel/:id/messages" element={<HotelMessages />} />
                      <Route path="/my-messages" element={<MyMessages />} />
                      <Route path="/admin/support-messages" element={<AdminSupportMessages />} />
                    </Routes>
                  </div>
                </Router>
              </CurrencyProvider>
            </FavoritesProvider>
          </AuthProvider>
        </LanguageProvider>
      </ThemeProvider>
    </GoogleOAuthProvider>
  );
}

export default App;
