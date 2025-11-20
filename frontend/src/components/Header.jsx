import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTheme } from '../context/ThemeContext';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import './Header.css';

const Header = () => {
  const [scrolled, setScrolled] = useState(false);
  const { theme, toggleTheme } = useTheme();
  const { language, toggleLanguage, t } = useLanguage();
  const { user, logout } = useAuth();

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

  return (
    <header className={`header ${scrolled ? 'scrolled' : ''}`}>
      <div className="header-container">
        <Link to="/" className="logo">
          Petoria<span>.</span>
        </Link>

        <nav>
          <ul className="nav-menu">
            <li><Link to="/" className="nav-link">{t('home')}</Link></li>
            <li><Link to="/hotels" className="nav-link">{t('hotels')}</Link></li>
            <li><Link to="/destinations" className="nav-link">{t('destinations')}</Link></li>
            <li><Link to="/about" className="nav-link">{t('about')}</Link></li>
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
