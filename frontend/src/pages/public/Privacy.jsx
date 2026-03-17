import React, { useState } from 'react';
import { useLanguage } from '../../context/LanguageContext';
import './Privacy.css';

/** Страница с политика за поверителност — accordion секции с правна информация. */
const Privacy = () => {
    const { t } = useLanguage();
    // Индекс на отворената accordion секция
    const [activeSection, setActiveSection] = useState(null);

    const sections = [
        {
            icon: '📋',
            titleKey: 'privacySection1Title',
            contentKey: 'privacySection1Text',
        },
        {
            icon: '🔍',
            titleKey: 'privacySection2Title',
            contentKey: 'privacySection2Text',
        },
        {
            icon: '🔒',
            titleKey: 'privacySection3Title',
            contentKey: 'privacySection3Text',
        },
        {
            icon: '🌐',
            titleKey: 'privacySection4Title',
            contentKey: 'privacySection4Text',
        },
        {
            icon: '🍪',
            titleKey: 'privacySection5Title',
            contentKey: 'privacySection5Text',
        },
        {
            icon: '✉️',
            titleKey: 'privacySection6Title',
            contentKey: 'privacySection6Text',
        },
        {
            icon: '⚖️',
            titleKey: 'privacySection7Title',
            contentKey: 'privacySection7Text',
        },
        {
            icon: '📞',
            titleKey: 'privacySection8Title',
            contentKey: 'privacySection8Text',
        },
    ];

    return (
        <div className="privacy-page">
            {/* Hero Section */}
            <section className="privacy-hero">
                <div className="privacy-hero-overlay">
                    <div className="privacy-hero-content">
                        <div className="privacy-hero-icon">🛡️</div>
                        <h1>{t('privacyTitle')}</h1>
                        <p>{t('privacySubtitle')}</p>
                        <div className="privacy-meta">
                            <span className="privacy-date">📅 {t('privacyLastUpdated')}: 01.03.2025</span>
                        </div>
                    </div>
                </div>
            </section>

            {/* Content */}
            <div className="privacy-container">

                {/* Intro */}
                <div className="privacy-intro">
                    <p>{t('privacyIntro')}</p>
                </div>

                {/* Table of Contents */}
                <div className="privacy-toc">
                    <h2>{t('privacyTableOfContents')}</h2>
                    <ul>
                        {sections.map((sec, i) => (
                            <li key={i}>
                                <a href={`#section-${i}`}>
                                    <span className="toc-num">{i + 1}.</span>
                                    {t(sec.titleKey)}
                                </a>
                            </li>
                        ))}
                    </ul>
                </div>

                {/* Sections */}
                <div className="privacy-sections">
                    {sections.map((sec, i) => (
                        <div
                            key={i}
                            id={`section-${i}`}
                            className={`privacy-section ${activeSection === i ? 'active' : ''}`}
                        >
                            <div
                                className="privacy-section-header"
                                onClick={() => setActiveSection(activeSection === i ? null : i)}
                            >
                                <div className="section-title-row">
                                    <span className="section-icon">{sec.icon}</span>
                                    <h3>{i + 1}. {t(sec.titleKey)}</h3>
                                </div>
                                <span className="section-toggle">{activeSection === i ? '▲' : '▼'}</span>
                            </div>
                            <div className={`privacy-section-body ${activeSection === i ? 'expanded' : ''}`}>
                                <p>{t(sec.contentKey)}</p>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Contact Banner */}
                <div className="privacy-contact-banner">
                    <div className="contact-banner-icon">💬</div>
                    <div className="contact-banner-text">
                        <h3>{t('privacyContactTitle')}</h3>
                        <p>{t('privacyContactText')}</p>
                    </div>
                    <a href="mailto:petooriaa@gmail.com" className="privacy-contact-btn">
                        {t('privacyContactBtn')}
                    </a>
                </div>

            </div>
        </div>
    );
};

export default Privacy;
