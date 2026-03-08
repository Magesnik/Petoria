import React, { useState, useEffect, useRef } from 'react';
import { api } from '../utils/api';
import { Link, useNavigate } from 'react-router-dom';
import { useTheme } from '../context/ThemeContext';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import { useCurrency } from '../context/CurrencyContext';
import { useCart } from '../context/CartContext';
import logoImg from '../assets/logo.png';
import './Header.css';

const Header = () => {
  const [scrolled, setScrolled] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const dropdownRef = useRef(null);
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();
  const { language, toggleLanguage, t } = useLanguage();
  const { currency, changeCurrency, availableCurrencies } = useCurrency();
  const { user, logout, isAdmin, isSuperAdmin } = useAuth();
  const { cartCount } = useCart();

  useEffect(() => {
    const handleScroll = () => {
      if (window.scrollY > 50) {
        setScrolled(true);
      } else {
        setScrolled(false);
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Fetch unread messages count and moderator status
  const [unreadCount, setUnreadCount] = useState(0);
  const [isModerator, setIsModerator] = useState(false);

  useEffect(() => {
    if (user) {
      fetchUserData();
      // Poll every minute
      const interval = setInterval(fetchUserData, 60000);
      return () => clearInterval(interval);
    }
  }, [user]);

  const fetchUserData = async () => {
    try {
      const messagesData = await api.get('/hotels/my/messages/unread-responses-count');

      let supportUnreadCount = 0;
      try {
        const supportMessagesData = await api.get('/support/messages/my/unread-count');
        supportUnreadCount = supportMessagesData.count || 0;
      } catch (e) {
        console.warn('Could not fetch support unread count', e);
      }

      setUnreadCount((messagesData.count || 0) + supportUnreadCount);

      // Check if user is a moderator for any hotel
      // We can use the /hotels/moderated endpoint. If it returns any hotels, they are a moderator.
      const moderatedHotels = await api.get('/hotels/moderated');
      setIsModerator(moderatedHotels.length > 0);
    } catch (err) {
      console.error('Error fetching user data:', err);
    }
  };

  // Click outside to close dropdown
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Close mobile menu on route change
  useEffect(() => {
    setMobileMenuOpen(false);
  }, [navigate]);

  // Prevent body scroll when mobile menu is open
  useEffect(() => {
    if (mobileMenuOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [mobileMenuOpen]);

  const handleLogout = () => {
    logout();
    setDropdownOpen(false);
    setMobileMenuOpen(false);
    navigate('/');
  };

  const toggleDropdown = () => {
    setDropdownOpen(!dropdownOpen);
  };

  const toggleMobileMenu = () => {
    setMobileMenuOpen(!mobileMenuOpen);
  };

  const closeMobileMenu = () => {
    setMobileMenuOpen(false);
  };

  return (
    <header className={`header ${scrolled ? 'scrolled' : ''}`}>
      <div className="header-container">
        <Link to="/" className="logo-link" onClick={closeMobileMenu}>
          <img src={logoImg} alt="Petoria Logo" className="logo-img" />
          <span className="logo-text">Petoria<span>.</span></span>
        </Link>

        {/* Desktop Navigation */}
        <nav className="desktop-nav">
          <ul className="nav-menu">
            <li><Link to="/" className="nav-link">{t('home')}</Link></li>
            <li><Link to="/hotels" className="nav-link">{t('hotels')}</Link></li>
            <li><Link to="/deals" className="nav-link">{t('deals')}</Link></li>
            <li><Link to="/about" className="nav-link">{t('about')}</Link></li>
            {isAdmin() && (
              <>
                <li><Link to="/my-hotels" className="nav-link admin-link">{t('myHotels')}</Link></li>
                <li><Link to="/create-hotel" className="nav-link admin-link">{t('createHotel')}</Link></li>
              </>
            )}
            {isSuperAdmin() && (
              <li><Link to="/admin" className="nav-link super-admin-link">🛡️ {t('adminPanel')}</Link></li>
            )}
            {isModerator && (
              <li><Link to="/moderator" className="nav-link moderator-link">🛡️ {t('moderatorPanel')}</Link></li>
            )}
          </ul>
        </nav>

        <div className="header-actions">
          {/* Global Settings (Visible only when not logged in) */}
          {!user && (
            <div className="global-settings">
              <button onClick={toggleTheme} className="btn-icon" title={theme === 'light' ? t('darkMode') : t('lightMode')}>
                {theme === 'light' ? '🌙' : '☀️'}
              </button>
              <button onClick={toggleLanguage} className="btn-icon" title={language === 'en' ? 'Български' : 'English'}>
                {language === 'en' ? '🇧🇬' : '🇬🇧'}
              </button>
              <button
                onClick={() => {
                  const nextIndex = (availableCurrencies.indexOf(currency) + 1) % availableCurrencies.length;
                  changeCurrency(availableCurrencies[nextIndex]);
                }}
                className="btn-icon"
                title={t('currency')}
              >
                {currency}
              </button>
            </div>
          )}

          {user ? (
            <div className="user-section" style={{ display: 'flex', alignItems: 'center' }}>
              <Link to="/cart" className="nav-notification" title={t('cart')}>
                🛒
                {cartCount > 0 && <span className="notification-count">{cartCount}</span>}
              </Link>
              <Link to="/my-messages" className="nav-notification">
                🔔
                {unreadCount > 0 && <span className="notification-count">{unreadCount}</span>}
              </Link>
              <div className="user-menu" ref={dropdownRef}>
                <button onClick={toggleDropdown} className="user-dropdown-trigger">
                  <div className="user-avatar-small">
                    {user.avatarUrl ? (
                      <img src={user.avatarUrl} alt={user.firstName} />
                    ) : (
                      <span>👤</span>
                    )}
                  </div>
                  <span className="user-name">{user.firstName || user.email}</span>
                  <span className={`dropdown-arrow ${dropdownOpen ? 'open' : ''}`}>▼</span>
                </button>
                {dropdownOpen && (
                  <div className="user-dropdown">
                    <Link to="/favorites" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                      <span className="dropdown-icon">❤️</span>
                      {t('favorites')}
                    </Link>
                    <Link to="/cart" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                      <span className="dropdown-icon">🛒</span>
                      {t('cart')}
                      {cartCount > 0 && (
                        <span className="dropdown-cart-badge">{cartCount}</span>
                      )}
                    </Link>
                    <Link to="/purchase-history" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                      <span className="dropdown-icon">🧾</span>
                      {t('purchaseHistory')}
                    </Link>
                    <Link to="/my-messages" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                      <span className="dropdown-icon">💬</span>
                      {t('myMessages') || 'My Messages'}
                    </Link>
                    <Link to="/settings" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                      <span className="dropdown-icon">⚙️</span>
                      {t('settings')}
                    </Link>
                    <Link to="/support" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                      <span className="dropdown-icon">❓</span>
                      {t('support')}
                    </Link>
                    <div className="dropdown-divider"></div>
                    <button onClick={handleLogout} className="dropdown-item logout-item">
                      <span className="dropdown-icon">🚪</span>
                      {t('logout')}
                    </button>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="auth-buttons-desktop">
              <Link to="/login" className="btn btn-login">{t('signIn')}</Link>
              <Link to="/register" className="btn btn-register">{t('register')}</Link>
            </div>
          )}

          {/* Hamburger Menu Button */}
          <button
            className={`hamburger-btn ${mobileMenuOpen ? 'open' : ''}`}
            onClick={toggleMobileMenu}
            aria-label="Toggle menu"
          >
            <span className="hamburger-line"></span>
            <span className="hamburger-line"></span>
            <span className="hamburger-line"></span>
          </button>
        </div>
      </div>

      {/* Mobile Navigation Overlay */}
      <div className={`mobile-nav-overlay ${mobileMenuOpen ? 'open' : ''}`} onClick={closeMobileMenu}></div>

      {/* Mobile Navigation Drawer */}
      <nav className={`mobile-nav ${mobileMenuOpen ? 'open' : ''}`}>
        <div className="mobile-nav-content">
          <ul className="mobile-nav-menu">
            <li><Link to="/" className="mobile-nav-link" onClick={closeMobileMenu}>{t('home')}</Link></li>
            <li><Link to="/hotels" className="mobile-nav-link" onClick={closeMobileMenu}>{t('hotels')}</Link></li>
            <li><Link to="/deals" className="mobile-nav-link" onClick={closeMobileMenu}>{t('deals')}</Link></li>
            <li><Link to="/about" className="mobile-nav-link" onClick={closeMobileMenu}>{t('about')}</Link></li>
            {isAdmin() && (
              <>
                <li><Link to="/my-hotels" className="mobile-nav-link admin-link" onClick={closeMobileMenu}>{t('myHotels')}</Link></li>
                <li><Link to="/create-hotel" className="mobile-nav-link admin-link" onClick={closeMobileMenu}>{t('createHotel')}</Link></li>
              </>
            )}
            {isSuperAdmin() && (
              <li><Link to="/admin" className="mobile-nav-link super-admin-link" onClick={closeMobileMenu}>🛡️ {t('adminPanel')}</Link></li>
            )}
            {isModerator && (
              <li><Link to="/moderator" className="mobile-nav-link moderator-link" onClick={closeMobileMenu}>🛡️ {t('moderatorPanel')}</Link></li>
            )}
          </ul>

          {user ? (
            <div className="mobile-user-section">
              <div className="mobile-user-info">
                <div className="user-avatar-small">
                  {user.avatarUrl ? (
                    <img src={user.avatarUrl} alt={user.firstName} />
                  ) : (
                    <span>👤</span>
                  )}
                </div>
                <span className="user-name">{user.firstName}</span>
              </div>
              <ul className="mobile-user-menu">
                <li><Link to="/favorites" className="mobile-nav-link" onClick={closeMobileMenu}>❤️ {t('favorites')}</Link></li>
                <li>
                  <Link to="/cart" className="mobile-nav-link" onClick={closeMobileMenu}>
                    🛒 {t('cart')}
                    {cartCount > 0 && <span className="mobile-cart-badge">{cartCount}</span>}
                  </Link>
                </li>
                <li>
                  <Link to="/my-messages" className="mobile-nav-link" onClick={closeMobileMenu}>
                    🔔 {t('myMessages') || 'My Messages'}
                    {unreadCount > 0 && <span className="mobile-cart-badge">{unreadCount}</span>}
                  </Link>
                </li>
                <li><Link to="/purchase-history" className="mobile-nav-link" onClick={closeMobileMenu}>🧾 {t('purchaseHistory')}</Link></li>
                <li><Link to="/settings" className="mobile-nav-link" onClick={closeMobileMenu}>⚙️ {t('settings')}</Link></li>
                <li><Link to="/support" className="mobile-nav-link" onClick={closeMobileMenu}>💬 {t('support')}</Link></li>
                <li><button onClick={handleLogout} className="mobile-nav-link logout-link">🚪 {t('logout')}</button></li>
              </ul>
            </div>
          ) : (
            <div className="mobile-auth-buttons">
              <Link to="/login" className="btn btn-login-mobile" onClick={closeMobileMenu}>{t('signIn')}</Link>
              <Link to="/register" className="btn btn-register-mobile" onClick={closeMobileMenu}>{t('register')}</Link>
            </div>
          )}
        </div>
      </nav>
    </header >
  );
};

export default Header;
