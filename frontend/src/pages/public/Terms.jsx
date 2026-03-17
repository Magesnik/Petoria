import React, { useState } from 'react';
import { useLanguage } from '../../context/LanguageContext';
import './Terms.css';

/** Страница с общи условия за ползване — accordion секции с правна информация. */
const Terms = () => {
    const { t } = useLanguage();
    // Индекс на отворената accordion секция
    const [activeSection, setActiveSection] = useState(null);

    const sections = [
        { icon: '📜', titleKey: 'termsSection1Title', contentKey: 'termsSection1Text' },
        { icon: '🔑', titleKey: 'termsSection2Title', contentKey: 'termsSection2Text' },
        { icon: '🏨', titleKey: 'termsSection3Title', contentKey: 'termsSection3Text' },
        { icon: '💳', titleKey: 'termsSection4Title', contentKey: 'termsSection4Text' },
        { icon: '❌', titleKey: 'termsSection5Title', contentKey: 'termsSection5Text' },
        { icon: '⚠️', titleKey: 'termsSection6Title', contentKey: 'termsSection6Text' },
        { icon: '🛡️', titleKey: 'termsSection7Title', contentKey: 'termsSection7Text' },
        { icon: '📝', titleKey: 'termsSection8Title', contentKey: 'termsSection8Text' },
        { icon: '⚖️', titleKey: 'termsSection9Title', contentKey: 'termsSection9Text' },
        { icon: '📞', titleKey: 'termsSection10Title', contentKey: 'termsSection10Text' },
    ];

    return (
        <div className="terms-page">
            {/* Hero */}
            <section className="terms-hero">
                <div className="terms-hero-overlay">
                    <div className="terms-hero-content">
                        <div className="terms-hero-icon">📋</div>
                        <h1>{t('termsTitle')}</h1>
                        <p>{t('termsSubtitle')}</p>
                        <div className="terms-meta">
                            <span className="terms-date">📅 {t('termsLastUpdated')}: 01.03.2025</span>
                        </div>
                    </div>
                </div>
            </section>

            {/* Content */}
            <div className="terms-container">

                {/* Intro */}
                <div className="terms-intro">
                    <p>{t('termsIntro')}</p>
                </div>

                {/* Table of Contents */}
                <div className="terms-toc">
                    <h2>{t('termsTableOfContents')}</h2>
                    <ul>
                        {sections.map((sec, i) => (
                            <li key={i}>
                                <a href={`#terms-section-${i}`}>
                                    <span className="toc-num">{i + 1}.</span>
                                    {t(sec.titleKey)}
                                </a>
                            </li>
                        ))}
                    </ul>
                </div>

                {/* Sections */}
                <div className="terms-sections">
                    {sections.map((sec, i) => (
                        <div
                            key={i}
                            id={`terms-section-${i}`}
                            className={`terms-section ${activeSection === i ? 'active' : ''}`}
                        >
                            <div
                                className="terms-section-header"
                                onClick={() => setActiveSection(activeSection === i ? null : i)}
                            >
                                <div className="section-title-row">
                                    <span className="section-icon">{sec.icon}</span>
                                    <h3>{i + 1}. {t(sec.titleKey)}</h3>
                                </div>
                                <span className="section-toggle">{activeSection === i ? '▲' : '▼'}</span>
                            </div>
                            <div className={`terms-section-body ${activeSection === i ? 'expanded' : ''}`}>
                                <p>{t(sec.contentKey)}</p>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Contact Banner */}
                <div className="terms-contact-banner">
                    <div className="contact-banner-icon">💬</div>
                    <div className="contact-banner-text">
                        <h3>{t('termsContactTitle')}</h3>
                        <p>{t('termsContactText')}</p>
                    </div>
                    <a href="mailto:petooriaa@gmail.com" className="terms-contact-btn">
                        {t('termsContactBtn')}
                    </a>
                </div>

            </div>
        </div>
    );
};

export default Terms;
