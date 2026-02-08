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
                            {t('footerDescription')}
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
                        <h3 className="footer-title">{t('navigation')}</h3>
                        <ul className="footer-links">
                            <li><Link to="/">{t('home')}</Link></li>
                            <li><Link to="/hotels">{t('hotels')}</Link></li>
                            <li><Link to="/deals">🎁 {t('deals')}</Link></li>
                            <li><Link to="/about">{t('about')}</Link></li>
                        </ul>
                    </div>

                    <div className="footer-column">
                        <h3 className="footer-title">{t('supportMenu')}</h3>
                        <ul className="footer-links">
                            <li><Link to="/support">{t('help')}</Link></li>
                            <li><Link to="/favorites">{t('favorites')}</Link></li>
                            <li><Link to="/purchase-history">{t('history')}</Link></li>
                            <li><Link to="/settings">{t('settings')}</Link></li>
                        </ul>
                    </div>

                    <div className="footer-column">
                        <h3 className="footer-title">{t('contacts')}</h3>
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
                                <span>{t('location')}</span>
                            </li>
                        </ul>
                    </div>
                </div>

                {/* Footer Bottom */}
                <div className="footer-bottom">
                    <p className="copyright">
                        © {currentYear} Petoria. {t('allRightsReserved')}.
                    </p>
                    <div className="footer-bottom-links">
                        <Link to="/privacy">{t('privacy')}</Link>
                        <Link to="/terms">{t('terms')}</Link>
                        <Link to="/cookies">{t('cookies')}</Link>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;
