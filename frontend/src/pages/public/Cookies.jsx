import React, { useState } from 'react';
import { useLanguage } from '../../context/LanguageContext';
import './Cookies.css';

/** Страница с политика за бисквитки — видове бисквитки и управление на предпочитания. */
const Cookies = () => {
    const { t } = useLanguage();
    // Индекс на отворената accordion секция
    const [activeSection, setActiveSection] = useState(null);

    const cookieTypes = [
        { icon: '⚙️', color: 'essential',  titleKey: 'cookieTypeEssentialTitle',  descKey: 'cookieTypeEssentialDesc',  required: true },
        { icon: '📊', color: 'analytics',  titleKey: 'cookieTypeAnalyticsTitle',  descKey: 'cookieTypeAnalyticsDesc',  required: false },
        { icon: '⚙️', color: 'functional', titleKey: 'cookieTypeFunctionalTitle', descKey: 'cookieTypeFunctionalDesc', required: false },
        { icon: '📢', color: 'marketing',  titleKey: 'cookieTypeMarketingTitle',  descKey: 'cookieTypeMarketingDesc',  required: false },
    ];

    const sections = [
        { icon: '🍪', titleKey: 'cookiesSection1Title', contentKey: 'cookiesSection1Text' },
        { icon: '🔧', titleKey: 'cookiesSection2Title', contentKey: 'cookiesSection2Text' },
        { icon: '🌐', titleKey: 'cookiesSection3Title', contentKey: 'cookiesSection3Text' },
        { icon: '⏱️', titleKey: 'cookiesSection4Title', contentKey: 'cookiesSection4Text' },
        { icon: '🛠️', titleKey: 'cookiesSection5Title', contentKey: 'cookiesSection5Text' },
        { icon: '📞', titleKey: 'cookiesSection6Title', contentKey: 'cookiesSection6Text' },
    ];

    return (
        <div className="cookies-page">
            {/* Hero */}
            <section className="cookies-hero">
                <div className="cookies-hero-overlay">
                    <div className="cookies-hero-content">
                        <div className="cookies-hero-icon">🍪</div>
                        <h1>{t('cookiesTitle')}</h1>
                        <p>{t('cookiesSubtitle')}</p>
                        <div className="cookies-meta">
                            <span className="cookies-date">📅 {t('cookiesLastUpdated')}: 01.03.2025</span>
                        </div>
                    </div>
                </div>
            </section>

            <div className="cookies-container">

                {/* Intro */}
                <div className="cookies-intro">
                    <p>{t('cookiesIntro')}</p>
                </div>

                {/* Cookie Types Grid */}
                <div className="cookies-types-section">
                    <h2 className="cookies-types-title">{t('cookiesTypesTitle')}</h2>
                    <div className="cookies-types-grid">
                        {cookieTypes.map((type, i) => (
                            <div key={i} className={`cookie-type-card cookie-type-${type.color}`}>
                                <div className="cookie-type-header">
                                    <span className="cookie-type-icon">{type.icon}</span>
                                    <h3>{t(type.titleKey)}</h3>
                                    {type.required && (
                                        <span className="cookie-required-badge">{t('cookiesRequired')}</span>
                                    )}
                                </div>
                                <p>{t(type.descKey)}</p>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Accordion Sections */}
                <div className="cookies-sections">
                    {sections.map((sec, i) => (
                        <div
                            key={i}
                            id={`cookie-section-${i}`}
                            className={`cookies-section ${activeSection === i ? 'active' : ''}`}
                        >
                            <div
                                className="cookies-section-header"
                                onClick={() => setActiveSection(activeSection === i ? null : i)}
                            >
                                <div className="section-title-row">
                                    <span className="section-icon">{sec.icon}</span>
                                    <h3>{t(sec.titleKey)}</h3>
                                </div>
                                <span className="section-toggle">{activeSection === i ? '▲' : '▼'}</span>
                            </div>
                            <div className={`cookies-section-body ${activeSection === i ? 'expanded' : ''}`}>
                                <p>{t(sec.contentKey)}</p>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Manage Preferences Banner */}
                <div className="cookies-manage-banner">
                    <div className="manage-banner-icon">⚙️</div>
                    <div className="manage-banner-text">
                        <h3>{t('cookiesManageTitle')}</h3>
                        <p>{t('cookiesManageText')}</p>
                    </div>
                    <button
                        className="cookies-manage-btn"
                        onClick={() => {
                            localStorage.removeItem('cookieConsent');
                            window.location.reload();
                        }}
                    >
                        {t('cookiesManageBtn')}
                    </button>
                </div>

            </div>
        </div>
    );
};

export default Cookies;
