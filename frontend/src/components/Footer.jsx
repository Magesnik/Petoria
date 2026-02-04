import React from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import './Footer.css';

const Footer = () => {
    const { t } = useLanguage();
    const currentYear = new Date().getFullYear();

    return (
        <footer className="footer">
            <div className="footer-container">
                {/* Footer Top */}
                <div className="footer-top">
                    <div className="footer-column">
                        <div className="footer-logo">
                            (Petoria)<span>.</span>
                        </div>
                        <p className="footer-description">
                            Открийте най-добрите хотели и настанявания за вашата следваща почивка.
                            Резервирайте лесно и бързо с Petoria.
                        </p>
                        <div className="social-links">
                            <a href="https://facebook.com" target="_blank" rel="noopener noreferrer" aria-label="Facebook">
                                <span>📘</span>
                            </a>
                            <a href="https://instagram.com" target="_blank" rel="noopener noreferrer" aria-label="Instagram">
                                <span>📷</span>
                            </a>
                            <a href="https://twitter.com" target="_blank" rel="noopener noreferrer" aria-label="Twitter">
                                <span>🐦</span>
                            </a>
                            <a href="https://linkedin.com" target="_blank" rel="noopener noreferrer" aria-label="LinkedIn">
                                <span>💼</span>
                            </a>
                        </div>
                    </div>

                    <div className="footer-column">
                        <h3 className="footer-title">Навигация</h3>
                        <ul className="footer-links">
                            <li><Link to="/">Начало</Link></li>
                            <li><Link to="/hotels">Хотели</Link></li>
                            <li><Link to="/deals">🎁 Оферти</Link></li>
                            <li><Link to="/about">За нас</Link></li>
                        </ul>
                    </div>

                    <div className="footer-column">
                        <h3 className="footer-title">Поддръжка</h3>
                        <ul className="footer-links">
                            <li><Link to="/support">Помощ</Link></li>
                            <li><Link to="/favorites">Любими</Link></li>
                            <li><Link to="/purchase-history">История</Link></li>
                            <li><Link to="/settings">Настройки</Link></li>
                        </ul>
                    </div>

                    <div className="footer-column">
                        <h3 className="footer-title">Контакти</h3>
                        <ul className="footer-contact">
                            <li>
                                <span className="contact-icon">📧</span>
                                <a href="mailto:info@petoria.com">info@petoria.com</a>
                            </li>
                            <li>
                                <span className="contact-icon">📱</span>
                                <a href="tel:+359888123456">+359 888 123 456</a>
                            </li>
                            <li>
                                <span className="contact-icon">📍</span>
                                <span>София, България</span>
                            </li>
                        </ul>
                    </div>
                </div>

                {/* Footer Bottom */}
                <div className="footer-bottom">
                    <p className="copyright">
                        © {currentYear} Petoria. Всички права запазени.
                    </p>
                    <div className="footer-bottom-links">
                        <Link to="/privacy">Поверителност</Link>
                        <Link to="/terms">Условия за ползване</Link>
                        <Link to="/cookies">Бисквитки</Link>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;
