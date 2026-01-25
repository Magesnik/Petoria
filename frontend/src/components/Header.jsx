import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTheme } from '../context/ThemeContext';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import './Header.css';

const Header = () => {
  const [scrolled, setScrolled] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const dropdownRef = useRef(null);
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();
  const { language, toggleLanguage, t } = useLanguage();
  const { user, logout, isAdmin, isSuperAdmin } = useAuth();

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
        <Link to="/" className="logo" onClick={closeMobileMenu}>
          (Petoria)<span>.</span>
        </Link>

        {/* Desktop Navigation */}
        <nav className="desktop-nav">
          <ul className="nav-menu">
            <li><Link to="/" className="nav-link">{t('home')}</Link></li>
            <li><Link to="/hotels" className="nav-link">{t('hotels')}</Link></li>
            <li><Link to="/destinations" className="nav-link">{t('destinations')}</Link></li>
            <li><Link to="/about" className="nav-link">{t('about')}</Link></li>
            {isAdmin() && (
              <>
                <li><Link to="/my-hotels" className="nav-link admin-link">🏨 Моите хотели</Link></li>
                <li><Link to="/create-hotel" className="nav-link admin-link">➕ Създай хотел</Link></li>
              </>
            )}
            {isSuperAdmin() && (
              <li><Link to="/admin" className="nav-link super-admin-link">🛡️ Admin Panel</Link></li>
            )}
          </ul>
        </nav>

        <div className="header-actions">
          <button onClick={toggleTheme} className="btn-icon">
            {theme === 'light' ? '🌙' : '☀️'}
          </button>
          <button onClick={toggleLanguage} className="btn-icon">
            {language === 'en' ? '🇧🇬' : '🇬🇧'}
          </button>
          {user ? (
            <div className="user-menu" ref={dropdownRef}>
              <button onClick={toggleDropdown} className="user-dropdown-trigger">
                <div className="user-avatar-small">
                  {user.avatarUrl ? (
                    <img src={user.avatarUrl} alt={user.firstName} />
                  ) : (
                    <span>👤</span>
                  )}
                </div>
                <span className="user-name">{user.firstName}</span>
                <span className={`dropdown-arrow ${dropdownOpen ? 'open' : ''}`}>▼</span>
              </button>
              {dropdownOpen && (
                <div className="user-dropdown">
                  <Link to="/favorites" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                    <span className="dropdown-icon">❤️</span>
                    {t('favorites')}
                  </Link>
                  <Link to="/purchase-history" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                    <span className="dropdown-icon">🛒</span>
                    {t('purchaseHistory')}
                  </Link>
                  <Link to="/settings" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                    <span className="dropdown-icon">⚙️</span>
                    {t('settings')}
                  </Link>
                  <Link to="/support" className="dropdown-item" onClick={() => setDropdownOpen(false)}>
                    <span className="dropdown-icon">💬</span>
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
            <li><Link to="/destinations" className="mobile-nav-link" onClick={closeMobileMenu}>{t('destinations')}</Link></li>
            <li><Link to="/about" className="mobile-nav-link" onClick={closeMobileMenu}>{t('about')}</Link></li>
            {isAdmin() && (
              <>
                <li><Link to="/my-hotels" className="mobile-nav-link admin-link" onClick={closeMobileMenu}>🏨 Моите хотели</Link></li>
                <li><Link to="/create-hotel" className="mobile-nav-link admin-link" onClick={closeMobileMenu}>➕ Създай хотел</Link></li>
              </>
            )}
            {isSuperAdmin() && (
              <li><Link to="/admin" className="mobile-nav-link super-admin-link" onClick={closeMobileMenu}>🛡️ Admin Panel</Link></li>
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
                <li><Link to="/purchase-history" className="mobile-nav-link" onClick={closeMobileMenu}>🛒 {t('purchaseHistory')}</Link></li>
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
    </header>
  );
};

export default Header;
