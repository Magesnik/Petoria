import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLanguage } from '../../context/LanguageContext';
import { api } from '../../utils/api';

import Footer from '../../components/Footer';
import SearchBar from '../../components/SearchBar';
import './Home.css';

const Home = () => {
    const { t } = useLanguage();
    const navigate = useNavigate();
    const [popularDestinations, setPopularDestinations] = useState([]);
    const [loadingDestinations, setLoadingDestinations] = useState(true);

    useEffect(() => {
        const fetchPopularDestinations = async () => {
            try {
                const data = await api.get('/hotels/popular-destinations');
                setPopularDestinations(data);
            } catch (error) {
                console.error("Failed to load popular destinations", error);
            } finally {
                setLoadingDestinations(false);
            }
        };

        fetchPopularDestinations();
    }, []);

    const handleDestinationClick = (hotelId) => {
        navigate(`/hotel/${hotelId}`);
    };

    return (
        <div className="home">


            {/* Hero Section */}
            <section className="hero">
                <div className="hero-content">
                    <h1>{t('heroTitle')} <br /> <span>{t('heroSubtitle')}</span></h1>
                    <p>{t('heroText')}</p>
                    <div className="search-container-wrapper" style={{ marginTop: '2rem' }}>
                        <SearchBar />
                    </div>
                </div>
            </section>

            {/* Featured Section */}
            <section className="featured container">
                <h2 className="section-title">{t('popularDestinations')}</h2>
                <div className="featured-grid">
                    {loadingDestinations ? (
                        /* Skeleton loaders */
                        Array.from({ length: 3 }).map((_, idx) => (
                            <div className="card" key={idx} style={{ opacity: 0.7 }}>
                                <div className="card-image" style={{ backgroundColor: '#e2e8f0' }}></div>
                                <div className="card-content">
                                    <h3 style={{ width: '60%', height: '24px', backgroundColor: '#e2e8f0', marginBottom: '10px' }}></h3>
                                    <p style={{ width: '100%', height: '16px', backgroundColor: '#e2e8f0', marginBottom: '5px' }}></p>
                                    <p style={{ width: '80%', height: '16px', backgroundColor: '#e2e8f0', marginBottom: '15px' }}></p>
                                    <div style={{ width: '40%', height: '20px', backgroundColor: '#e2e8f0' }}></div>
                                </div>
                            </div>
                        ))
                    ) : popularDestinations.length > 0 ? (
                        popularDestinations.map((dest, idx) => (
                            <div
                                className="card"
                                key={idx}
                                onClick={() => handleDestinationClick(dest.hotelId)}
                                style={{ cursor: 'pointer' }}
                            >
                                <div className="card-image" style={{ backgroundImage: `url('${dest.imageUrl}')` }}></div>
                                <div className="card-content">
                                    <h3>{dest.city}, {dest.country}</h3>
                                    <p>{t('discoverStaysIn')} {dest.city}.</p>
                                    <div className="card-price">{t('from')} ${dest.startingPrice}/{t('night')}</div>
                                </div>
                            </div>
                        ))
                    ) : (
                        <p>{t('noDestinationsFound', 'No destinations found.')}</p>
                    )}
                </div>
            </section>

            <Footer />
        </div>
    );
};

export default Home;

