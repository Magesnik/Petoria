import React, { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useTheme } from '../context/ThemeContext';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import './Header.css';

const Header = () => {
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef(null);
  const { theme, toggleTheme } = useTheme();
  const { language, toggleLanguage, t } = useLanguage();
  const { user, logout, isAdmin } = useAuth();

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

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setMenuOpen(false);
      }
    };

    if (menuOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [menuOpen]);

  const toggleMenu = () => {
    setMenuOpen(!menuOpen);
  };

  const closeMenu = () => {
    setMenuOpen(false);
  };

  const menuItems = [
    { path: '/', label: 'Начална страница', icon: '🏠' },
    { path: '/reservations', label: 'История на резервации', icon: '📋' },
    { path: '/paid', label: 'Вече платени', icon: '✅' },
    { path: '/profile', label: 'Профил', icon: '👤' },
    { path: '/planned', label: 'Планирани', icon: '📅' },
    { path: '/settings', label: 'Настройки', icon: '⚙️' },
    { path: '/support', label: 'Support', icon: '💬' },
  ];

  return (
    <header className={`header ${scrolled ? 'scrolled' : ''}`}>
      <div className="header-container">
        <Link to="/" className="logo">
          (Petoria)<span>.</span>
        </Link>

        <nav>
          <ul className="nav-menu">
            <li><Link to="/" className="nav-link">{t('home')}</Link></li>
            <li><Link to="/hotels" className="nav-link">{t('hotels')}</Link></li>
            <li><Link to="/destinations" className="nav-link">{t('destinations')}</Link></li>
            <li><Link to="/about" className="nav-link">{t('about')}</Link></li>
            {isAdmin() && (
              <li><Link to="/create-hotel" className="nav-link admin-link">➕ Създай хотел</Link></li>
            )}
          </ul>
        </nav>

        <div className="header-actions">
          {/* Menu Dropdown Button */}
          <div className="menu-dropdown" ref={menuRef}>
            <button onClick={toggleMenu} className="btn-icon menu-btn" aria-label="Menu">
              <span className="menu-icon">☰</span>
            </button>

            {menuOpen && (
              <div className="dropdown-menu">
                <div className="dropdown-header">
                  <h3>Меню</h3>
                  <button onClick={closeMenu} className="close-btn">✕</button>
                </div>
                <ul className="dropdown-list">
                  {menuItems.map((item, index) => (
                    <li key={index}>
                      <Link
                        to={item.path}
                        className="dropdown-item"
                        onClick={closeMenu}
                      >
                        <span className="dropdown-icon">{item.icon}</span>
                        <span className="dropdown-label">{item.label}</span>
                      </Link>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>

          <button onClick={toggleTheme} className="btn-icon">
            {theme === 'light' ? '🌙' : '☀️'}
          </button>
          <button onClick={toggleLanguage} className="btn-icon">
            {language === 'en' ? '🇧🇬' : '🇬🇧'}
          </button>
          {user ? (
            <div className="user-menu">
              <span className="user-name">{user.firstName}</span>
              <button onClick={logout} className="btn btn-logout">{t('logout') || 'Logout'}</button>
            </div>
          ) : (
            <>
              <Link to="/login" className="btn btn-login">{t('signIn')}</Link>
              <Link to="/register" className="btn btn-register">{t('register')}</Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;
