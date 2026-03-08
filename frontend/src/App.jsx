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
import ConfirmEmail from './pages/auth/ConfirmEmail';

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
import ProtectedRoute from './components/ProtectedRoute';
import { LiveUsersProvider } from './context/LiveUsersContext';

import ScrollToTop from './components/ScrollToTop';

function App() {
  return (
    <GoogleOAuthProvider clientId="21847094498-c94136osjkahal0fjg0nk9q4mc7e4um9.apps.googleusercontent.com">
      <AuthProvider>
        <ThemeProvider>
          <LanguageProvider>
            <CurrencyProvider>
              <FavoritesProvider>
                <CartProvider>
                  <LiveUsersProvider>
                    <Router>
                      <ScrollToTop />
                      <div className="App">
                        <Header /> {/* Added Global Header */}
                        <Routes>
                          {/* Public routes */}
                          <Route path="/" element={<Home />} />
                          <Route path="/hotels" element={<Hotels />} />
                          <Route path="/hotels/:id" element={<HotelDetails />} />
                          <Route path="/hotel/:id" element={<HotelDetails />} />
                          <Route path="/deals" element={<Deals />} />
                          <Route path="/about" element={<About />} />
                          <Route path="/login" element={<Login />} />
                          <Route path="/register" element={<Register />} />
                          <Route path="/confirm-email" element={<ConfirmEmail />} />
                          <Route path="/support" element={<Support />} />
                          <Route path="/hotel/:id/contact" element={<ContactHotel />} />
                          <Route path="/payment/success" element={<PaymentSuccess />} />
                          <Route path="/payment/cancel" element={<PaymentCancel />} />
                          <Route path="/privacy" element={<Privacy />} />
                          <Route path="/terms" element={<Terms />} />
                          <Route path="/cookies" element={<Cookies />} />

                          {/* Protected user routes */}
                          <Route path="/create-hotel" element={<ProtectedRoute><CreateHotel /></ProtectedRoute>} />
                          <Route path="/settings" element={<ProtectedRoute><Settings /></ProtectedRoute>} />
                          <Route path="/favorites" element={<ProtectedRoute><Favorites /></ProtectedRoute>} />
                          <Route path="/purchase-history" element={<ProtectedRoute><PurchaseHistory /></ProtectedRoute>} />
                          <Route path="/cart" element={<ProtectedRoute><Cart /></ProtectedRoute>} />
                          <Route path="/my-hotels" element={<ProtectedRoute><MyHotels /></ProtectedRoute>} />
                          <Route path="/manage-hotel/:id" element={<ProtectedRoute><ManageHotel /></ProtectedRoute>} />
                          <Route path="/hotel/:id/messages" element={<ProtectedRoute><HotelMessages /></ProtectedRoute>} />
                          <Route path="/my-messages" element={<ProtectedRoute><MyMessages /></ProtectedRoute>} />

                          {/* Protected Moderator routes */}
                          <Route path="/moderator" element={<ProtectedRoute requireModerator={true}><ModeratorDashboard /></ProtectedRoute>} />
                          <Route path="/moderator/hotel/:id" element={<ProtectedRoute requireModerator={true}><ModeratorHotelPanel /></ProtectedRoute>} />

                          {/* Protected Admin routes */}
                          <Route path="/admin" element={<ProtectedRoute requireAdmin={true}><AdminDashboard /></ProtectedRoute>} />
                          <Route path="/admin/support-messages" element={<ProtectedRoute requireAdmin={true}><AdminSupportMessages /></ProtectedRoute>} />
                        </Routes>
                        <Footer />
                        <CookieBanner />
                      </div>
                    </Router>
                  </LiveUsersProvider>
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
