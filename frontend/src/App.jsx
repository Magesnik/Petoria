import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { ThemeProvider } from './context/ThemeContext';
import { LanguageProvider } from './context/LanguageContext';
import { AuthProvider } from './context/AuthContext';
import { FavoritesProvider } from './context/FavoritesContext';
import { CurrencyProvider } from './context/CurrencyContext';
import { CartProvider } from './context/CartContext';
// Auth
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';

// Admin
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminSupportMessages from './pages/admin/AdminSupportMessages';

// Moderator
import ModeratorDashboard from './pages/moderator/ModeratorDashboard';
import ModeratorHotelPanel from './pages/moderator/ModeratorHotelPanel';

// Hotels
import Hotels from './pages/hotels/Hotels';
import HotelDetails from './pages/hotels/HotelDetails';
import CreateHotel from './pages/hotels/CreateHotel';
import ManageHotel from './pages/hotels/ManageHotel';
import MyHotels from './pages/hotels/MyHotels';
import ContactHotel from './pages/hotels/ContactHotel';
import HotelMessages from './pages/hotels/HotelMessages';

// User
import Cart from './pages/user/Cart';
import Settings from './pages/user/Settings';
import Favorites from './pages/user/Favorites';
import PurchaseHistory from './pages/user/PurchaseHistory';
import MyMessages from './pages/user/MyMessages';
import PaymentSuccess from './pages/user/PaymentSuccess';
import PaymentCancel from './pages/user/PaymentCancel';

// Public
import Home from './pages/public/Home';
import About from './pages/public/About';
import Deals from './pages/public/Deals';
import Support from './pages/public/Support';
import Privacy from './pages/public/Privacy';
import Terms from './pages/public/Terms';
import Cookies from './pages/public/Cookies';

import Header from './components/Header';
import Footer from './components/Footer';
import CookieBanner from './components/CookieBanner';

function App() {
  return (
    <GoogleOAuthProvider clientId="21847094498-c94136osjkahal0fjg0nk9q4mc7e4um9.apps.googleusercontent.com">
      <AuthProvider>
        <ThemeProvider>
          <LanguageProvider>
            <CurrencyProvider>
              <FavoritesProvider>
                <CartProvider>
                  <Router>
                    <div className="App">
                      <Header /> {/* Added Global Header */}
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
                        <Route path="/moderator" element={<ModeratorDashboard />} />
                        <Route path="/moderator/hotel/:id" element={<ModeratorHotelPanel />} />
                        <Route path="/my-hotels" element={<MyHotels />} />
                        <Route path="/manage-hotel/:id" element={<ManageHotel />} />
                        <Route path="/hotel/:id" element={<HotelDetails />} />
                        <Route path="/hotel/:id/contact" element={<ContactHotel />} />
                        <Route path="/hotel/:id/messages" element={<HotelMessages />} />
                        <Route path="/my-messages" element={<MyMessages />} />
                        <Route path="/admin/support-messages" element={<AdminSupportMessages />} />
                        <Route path="/cart" element={<Cart />} />
                        <Route path="/payment/success" element={<PaymentSuccess />} />
                        <Route path="/payment/cancel" element={<PaymentCancel />} />
                        <Route path="/privacy" element={<Privacy />} />
                        <Route path="/terms" element={<Terms />} />
                        <Route path="/cookies" element={<Cookies />} />
                      </Routes>
                      <Footer />
                      <CookieBanner />
                    </div>
                  </Router>
                </CartProvider>
              </FavoritesProvider>
            </CurrencyProvider>
          </LanguageProvider>
        </ThemeProvider>
      </AuthProvider>
    </GoogleOAuthProvider>
  );
}

export default App;
