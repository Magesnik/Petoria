import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTheme } from '../context/ThemeContext';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import './Header.css';

const Header = () => {
  const [scrolled, setScrolled] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const dropdownRef = useRef(null);
  const navigate = useNavigate();
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

  const handleLogout = () => {
    logout();
    setDropdownOpen(false);
    navigate('/');
  };

  const toggleDropdown = () => {
    setDropdownOpen(!dropdownOpen);
  };

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
          <button onClick={toggleTheme} className="btn-icon">
            {theme === 'light' ? '🌙' : '☀️'}
          </button>
          <button onClick={toggleLanguage} className="btn-icon">
            {language === 'en' ? '🇧🇬' : '🇬🇧'}
          </button>
          {user ? (
            <div className="user-menu" ref={dropdownRef}>
              <button onClick={toggleDropdown} className="user-dropdown-trigger">
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
