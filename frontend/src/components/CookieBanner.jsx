import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import './CookieBanner.css';

const CookieBanner = () => {
    const { t } = useLanguage();
    const [visible, setVisible] = useState(false);
    const [animOut, setAnimOut] = useState(false);

    useEffect(() => {
        const consent = localStorage.getItem('cookieConsent');
        if (!consent) {
            // Small delay so the page loads first
            const timer = setTimeout(() => setVisible(true), 800);
            return () => clearTimeout(timer);
        }
    }, []);

    const handleAccept = () => {
        localStorage.setItem('cookieConsent', 'accepted');
        dismiss();
    };

    const handleDecline = () => {
        localStorage.setItem('cookieConsent', 'essential-only');
        dismiss();
    };

    const dismiss = () => {
        setAnimOut(true);
        setTimeout(() => setVisible(false), 400);
    };

    if (!visible) return null;

    return (
        <div className={`cookie-banner ${animOut ? 'cookie-banner--out' : 'cookie-banner--in'}`} role="dialog" aria-label="Cookie consent">
            <div className="cookie-banner__inner">
                <div className="cookie-banner__icon">🍪</div>
                <div className="cookie-banner__text">
                    <h3>{t('cookieBannerTitle')}</h3>
                    <p>
                        {t('cookieBannerText')}{' '}
                        <Link to="/cookies">{t('cookieBannerLearnMore')}</Link>
                    </p>
                </div>
                <div className="cookie-banner__actions">
                    <button className="cookie-btn cookie-btn--accept" onClick={handleAccept}>
                        {t('cookieBannerAccept')}
                    </button>
                    <button className="cookie-btn cookie-btn--decline" onClick={handleDecline}>
                        {t('cookieBannerDecline')}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CookieBanner;
