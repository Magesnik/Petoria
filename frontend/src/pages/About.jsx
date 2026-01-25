import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import Footer from '../components/Footer';
import './About.css';

const About = () => {
    const { t } = useLanguage();
    const navigate = useNavigate();

    const values = [
        { icon: '⭐', title: t('valueQuality'), text: t('valueQualityText') },
        { icon: '🤝', title: t('valueTrust'), text: t('valueTrustText') },
        { icon: '💡', title: t('valueInnovation'), text: t('valueInnovationText') },
        { icon: '🌱', title: t('valueSustainability'), text: t('valueSustainabilityText') }
    ];

    const team = [
        { name: t('teamMember1Name'), role: t('teamMember1Role'), avatar: '👨‍💼' },
        { name: t('teamMember2Name'), role: t('teamMember2Role'), avatar: '👩‍💼' },
        { name: t('teamMember3Name'), role: t('teamMember3Role'), avatar: '👨‍💻' },
        { name: t('teamMember4Name'), role: t('teamMember4Role'), avatar: '👩‍💻' }
    ];

    return (
        <div className="about-page">
            <Header />

            {/* Hero Section */}
            <section className="about-hero">
                <div className="hero-overlay">
                    <div className="hero-content">
                        <h1 className="hero-title">{t('aboutUsHero')}</h1>
                        <p className="hero-subtitle">{t('aboutUsSubtitle')}</p>
                    </div>
                </div>
            </section>

            {/* Main Content */}
            <div className="about-content container">
                {/* Our Story Section */}
                <section className="story-section">
                    <div className="section-header">
                        <h2>{t('ourStory')}</h2>
                        <div className="header-line"></div>
                    </div>
                    <p className="story-text">{t('ourStoryText')}</p>
                </section>

                {/* Mission & Vision Section */}
                <section className="mission-vision-section">
                    <div className="mission-vision-grid">
                        <div className="mission-card">
                            <div className="card-icon">🎯</div>
                            <h3>{t('ourMission')}</h3>
                            <p>{t('ourMissionText')}</p>
                        </div>
                        <div className="vision-card">
                            <div className="card-icon">🚀</div>
                            <h3>{t('ourVision')}</h3>
                            <p>{t('ourVisionText')}</p>
                        </div>
                    </div>
                </section>

                {/* Values Section */}
                <section className="values-section">
                    <div className="section-header">
                        <h2>{t('ourValues')}</h2>
                        <div className="header-line"></div>
                    </div>
                    <div className="values-grid">
                        {values.map((value, index) => (
                            <div key={index} className="value-card">
                                <div className="value-icon">{value.icon}</div>
                                <h3>{value.title}</h3>
                                <p>{value.text}</p>
                            </div>
                        ))}
                    </div>
                </section>

                {/* Team Section */}
                <section className="team-section">
                    <div className="section-header">
                        <h2>{t('meetOurTeam')}</h2>
                        <div className="header-line"></div>
                    </div>
                    <div className="team-grid">
                        {team.map((member, index) => (
                            <div key={index} className="team-card">
                                <div className="team-avatar">{member.avatar}</div>
                                <h3 className="team-name">{member.name}</h3>
                                <p className="team-role">{member.role}</p>
                            </div>
                        ))}
                    </div>
                </section>

                {/* CTA Section */}
                <section className="cta-section">
                    <h2>{t('joinUs')}</h2>
                    <p>{t('joinUsText')}</p>
                    <div className="cta-buttons">
                        <button
                            className="btn-primary"
                            onClick={() => navigate('/hotels')}
                        >
                            {t('exploreHotels')}
                        </button>
                        <button className="btn-secondary">
                            {t('contactUs')}
                        </button>
                    </div>
                </section>
            </div>

            <Footer />
        </div>
    );
};

export default About;
