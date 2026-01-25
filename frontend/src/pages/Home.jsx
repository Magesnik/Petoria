import React from 'react';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import Footer from '../components/Footer';
import './Home.css';

const Home = () => {
    const { t } = useLanguage();

    return (
        <div className="home">
            <Header />

            {/* Hero Section */}
            <section className="hero">
                <div className="hero-content">
                    <h1>{t('heroTitle')} <br /> <span>{t('heroSubtitle')}</span></h1>
                    <p>{t('heroText')}</p>

                    <div className="search-box">
                        <input
                            type="text"
                            className="search-input"
                            placeholder={t('searchPlaceholder')}
                        />
                        <button className="btn-search">{t('searchBtn')}</button>
                    </div>
                </div>
            </section>

            {/* Featured Section */}
            <section className="featured container">
                <h2 className="section-title">{t('popularDestinations')}</h2>
                <div className="featured-grid">
                    <div className="card">
                        <div className="card-image" style={{ backgroundImage: "url('https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80')" }}></div>
                        <div className="card-content">
                            <h3>Maldives</h3>
                            <p>Experience the ultimate luxury in overwater bungalows.</p>
                            <div className="card-price">{t('from')} $250/{t('night')}</div>
                        </div>
                    </div>

                    <div className="card">
                        <div className="card-image" style={{ backgroundImage: "url('https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80')" }}></div>
                        <div className="card-content">
                            <h3>Santorini, Greece</h3>
                            <p>Breathtaking views and white-washed architecture.</p>
                            <div className="card-price">{t('from')} $180/{t('night')}</div>
                        </div>
                    </div>

                    <div className="card">
                        <div className="card-image" style={{ backgroundImage: "url('https://images.unsplash.com/photo-1506929562872-bb421503ef21?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80')" }}></div>
                        <div className="card-content">
                            <h3>Bali, Indonesia</h3>
                            <p>Tropical paradise with rich culture and stunning beaches.</p>
                            <div className="card-price">{t('from')} $120/{t('night')}</div>
                        </div>
                    </div>
                </div>
            </section>

            <Footer />
        </div>
    );
};

export default Home;

