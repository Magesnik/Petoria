import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Header from '../components/Header';
import Footer from '../components/Footer';
import './Deals.css';

const Deals = () => {
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState('discounted');
    const [deals, setDeals] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchDeals(activeTab);
    }, [activeTab]);

    const fetchDeals = async (type) => {
        setLoading(true);
        setError(null);

        try {
            const response = await fetch(`http://localhost:5150/api/deals/${type}`);

            if (!response.ok) {
                throw new Error('Failed to fetch deals');
            }

            const data = await response.json();
            setDeals(data);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching deals:', err);
        } finally {
            setLoading(false);
        }
    };

    const formatCountdown = (hours) => {
        if (hours <= 0) return 'Изтекло';
        if (hours < 24) return `${hours}ч`;
        const days = Math.floor(hours / 24);
        const remainingHours = hours % 24;
        return `${days}д ${remainingHours}ч`;
    };

    const handleHotelClick = (hotelId) => {
        navigate(`/hotels/${hotelId}`);
    };

    const tabs = [
        { id: 'discounted', label: '🏷️ Отстъпки', icon: '🏷️' },
        { id: 'last-minute', label: '⏰ Last Minute', icon: '⏰' },
        { id: 'seasonal', label: '🎉 Сезонни', icon: '🎉' },
        { id: 'packages', label: '💝 Пакети', icon: '💝' }
    ];

    return (
        <div className="deals-page">
            <Header />

            {/* Hero Section */}
            <div className="deals-hero">
                <div className="deals-hero-content">
                    <h1 className="deals-title">🎁 Специални Оферти</h1>
                    <p className="deals-subtitle">
                        Открийте невероятни промоции и спестете повече
                    </p>
                </div>
            </div>

            {/* Main Content */}
            <div className="deals-container">
                {/* Tabs */}
                <div className="deals-tabs">
                    {tabs.map(tab => (
                        <button
                            key={tab.id}
                            className={`tab-btn ${activeTab === tab.id ? 'active' : ''}`}
                            onClick={() => setActiveTab(tab.id)}
                        >
                            <span className="tab-icon">{tab.icon}</span>
                            <span className="tab-label">{tab.label}</span>
                        </button>
                    ))}
                </div>

                {/* Content */}
                {loading ? (
                    <div className="deals-loading">
                        <div className="spinner"></div>
                        <p>Зареждане на оферти...</p>
                    </div>
                ) : error ? (
                    <div className="deals-error">
                        <p>❌ {error}</p>
                    </div>
                ) : deals.length === 0 ? (
                    <div className="deals-empty">
                        <p>Няма налични оферти в момента</p>
                    </div>
                ) : (
                    <div className="deals-grid">
                        {deals.map(deal => (
                            <div
                                key={deal.id}
                                className="deal-card"
                                onClick={() => handleHotelClick(deal.id)}
                            >
                                {/* Discount Badge */}
                                <div className="discount-badge">
                                    -{deal.discountPercentage}%
                                </div>

                                {/* Countdown (Last-Minute only) */}
                                {activeTab === 'last-minute' && deal.hoursLeft && (
                                    <div className="countdown-badge">
                                        ⏰ {formatCountdown(deal.hoursLeft)}
                                    </div>
                                )}

                                {/* Season Badge (Seasonal only) */}
                                {activeTab === 'seasonal' && deal.season && (
                                    <div className="season-badge">
                                        {deal.season}
                                    </div>
                                )}

                                {/* Image */}
                                <div
                                    className="deal-image"
                                    style={{
                                        backgroundImage: `url(${deal.imageUrl || 'https://images.unsplash.com/photo-1566073771259-6a8506099945'})`
                                    }}
                                >
                                    <div className="deal-overlay"></div>
                                </div>

                                {/* Content */}
                                <div className="deal-content">
                                    <h3 className="deal-name">{deal.name}</h3>
                                    <p className="deal-location">
                                        📍 {deal.city}, {deal.country}
                                    </p>

                                    {/* Rating */}
                                    {deal.rating > 0 && (
                                        <div className="deal-rating">
                                            ⭐ {deal.rating.toFixed(1)}
                                        </div>
                                    )}

                                    {/* Price */}
                                    <div className="deal-price">
                                        {activeTab === 'packages' ? (
                                            <>
                                                <div className="price-row">
                                                    <span className="original-price">
                                                        ${deal.originalPackagePrice?.toFixed(0)}
                                                    </span>
                                                    <span className="discounted-price">
                                                        ${deal.discountedPackagePrice?.toFixed(0)}
                                                    </span>
                                                </div>
                                                <p className="package-info">
                                                    {deal.packageNights} нощувки • Спестете ${deal.saveAmount?.toFixed(0)}
                                                </p>
                                            </>
                                        ) : (
                                            <>
                                                <div className="price-row">
                                                    <span className="original-price">
                                                        ${deal.originalPrice?.toFixed(0)}
                                                    </span>
                                                    <span className="discounted-price">
                                                        ${deal.discountedPrice?.toFixed(0)}
                                                    </span>
                                                </div>
                                                <p className="price-label">
                                                    на нощувка • Спестете ${deal.saveAmount?.toFixed(0)}
                                                </p>
                                            </>
                                        )}
                                    </div>

                                    {/* CTA */}
                                    <button className="btn-book-deal">
                                        Виж офертата →
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            <Footer />
        </div>
    );
};

export default Deals;
