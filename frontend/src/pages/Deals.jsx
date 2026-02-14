import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCurrency } from '../context/CurrencyContext';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import Footer from '../components/Footer';
import './Deals.css';

const Deals = () => {
    const navigate = useNavigate();
    const { convertAndFormat } = useCurrency();
    const { t } = useLanguage();
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
        if (hours <= 0) return t('expired');
        if (hours < 24) return `${hours}ч`;
        const days = Math.floor(hours / 24);
        const remainingHours = hours % 24;
        return `${days}д ${remainingHours}ч`;
    };

    const handleHotelClick = (hotelId) => {
        navigate(`/hotels/${hotelId}`);
    };

    const tabs = [
        { id: 'discounted', label: t('discounts'), icon: '🏷️' },
        { id: 'last-minute', label: t('lastMinute'), icon: '⏰' },
        { id: 'seasonal', label: t('seasonal'), icon: '🎉' },
        { id: 'packages', label: t('packages'), icon: '💝' }
    ];

    return (
        <div className="deals-page">
            <Header />

            {/* Hero Section */}
            <div className="deals-hero">
                <div className="deals-hero-content">
                    <h1 className="deals-title">{t('specialOffers')}</h1>
                    <p className="deals-subtitle">
                        {t('discoverDeals')}
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
                                key={deal.id || deal.roomTypeId}
                                className="deal-card"
                                onClick={() => handleHotelClick(deal.id || deal.hotelId)}
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
                                        backgroundImage: `url(${deal.imageUrl || deal.hotelImageUrl || 'https://images.unsplash.com/photo-1566073771259-6a8506099945'})`
                                    }}
                                >
                                    <div className="deal-overlay"></div>
                                </div>

                                {/* Content */}
                                <div className="deal-content">
                                    {activeTab === 'last-minute' ? (
                                        // Room-specific layout for Last Minute
                                        <>
                                            <h3 className="deal-name">{deal.hotelName}</h3>
                                            <p className="deal-location">
                                                📍 {deal.hotelCity}, {deal.hotelCountry}
                                            </p>

                                            {/* Room Type Info */}
                                            <div className="room-type-info">
                                                <h4 className="room-type-name">{deal.roomTypeName}</h4>
                                                <p className="room-capacity">👥 {deal.capacity} {t('people')}</p>
                                            </div>

                                            {/* Urgency indicator */}
                                            <div className="urgency-badge">
                                                {deal.availableRoomsCount === 1 ? (
                                                    <span className="critical">🔥 {t('lastRoom')}</span>
                                                ) : (
                                                    <span>⚠️ Само {deal.availableRoomsCount} стаи</span>
                                                )}
                                            </div>

                                            {/* Check-in date */}
                                            <p className="checkin-date">
                                                📅 {t('checkInColon')} {new Date(deal.earliestAvailableDate).toLocaleDateString('bg-BG')}
                                            </p>

                                            {/* 5% discount note if automatic */}
                                            {deal.discountPercentage === 5 && (
                                                <p className="auto-discount-note">
                                                    💡 Специална 5% отстъпка за първите 2 нощувки!
                                                </p>
                                            )}

                                            {/* Rating */}
                                            {deal.hotelRating > 0 && (
                                                <div className="deal-rating">
                                                    ⭐ {deal.hotelRating.toFixed(1)}
                                                </div>
                                            )}

                                            {/* Price */}
                                            <div className="deal-price">
                                                <div className="price-row">
                                                    <span className="original-price">
                                                        {convertAndFormat(deal.originalPrice)}
                                                    </span>
                                                    <span className="discounted-price">
                                                        {convertAndFormat(deal.discountedPrice)}
                                                    </span>
                                                </div>
                                                <p className="price-label">
                                                    {t('perNightSave')} {convertAndFormat(deal.saveAmount)}
                                                </p>
                                            </div>

                                            {/* CTA */}
                                            <button
                                                className="btn-book-deal"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleHotelClick(deal.hotelId);
                                                }}
                                            >
                                                {t('bookNowArrow') || t('bookNow')}
                                            </button>
                                        </>
                                    ) : (
                                        // Original layout for other tabs
                                        <>
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
                                                            {deal.packageNights} {t('nightsSave')} ${deal.saveAmount?.toFixed(0)}
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
                                                            {t('perNightSave')} ${deal.saveAmount?.toFixed(0)}
                                                        </p>
                                                    </>
                                                )}
                                            </div>

                                            {/* CTA */}
                                            <button className="btn-book-deal">
                                                {t('viewDeal')}
                                            </button>
                                        </>
                                    )}
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
