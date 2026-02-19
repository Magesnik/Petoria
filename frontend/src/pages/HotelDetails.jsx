import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import { useFavorites } from '../context/FavoritesContext';
import { useAuth } from '../context/AuthContext';
import { useCurrency } from '../context/CurrencyContext';

import ReviewSection from '../components/ReviewSection';
import BookingWidget from '../components/BookingWidget';
import RoomTypeManager from '../components/RoomTypeManager';
import './HotelDetails.css';

const HotelDetails = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useLanguage();
    const { isFavorite, toggleFavorite } = useFavorites();
    const { user, isAdmin } = useAuth();
    const { convertAndFormat } = useCurrency();
    const [hotel, setHotel] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [currentImageIndex, setCurrentImageIndex] = useState(0);



    useEffect(() => {
        fetchHotelDetails();
    }, [id]);

    const fetchHotelDetails = async () => {
        setLoading(true);
        setError(null);

        try {
            const data = await api.get(`/hotels/${id}`);
            setHotel(data);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching hotel details:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleBackClick = () => {
        navigate('/hotels');
    };

    // Get all hotel images (main + additional)
    const getHotelImages = () => {
        if (!hotel) return [];
        const additionalImages = hotel.images ? JSON.parse(hotel.images) : [];
        const mainImage = hotel.imageUrl || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=1600&q=80';
        return [mainImage, ...additionalImages];
    };

    const handlePreviousImage = () => {
        if (!hotel) return;
        const images = getHotelImages();
        setCurrentImageIndex((prev) => (prev === 0 ? images.length - 1 : prev - 1));
    };

    const handleNextImage = () => {
        if (!hotel) return;
        const images = getHotelImages();
        setCurrentImageIndex((prev) => (prev === images.length - 1 ? 0 : prev + 1));
    };

    const handleThumbnailClick = (index) => {
        setCurrentImageIndex(index);
    };

    const handleFavoriteClick = () => {
        toggleFavorite(parseInt(id));
    };

    if (loading) {
        return (
            <div className="hotel-details-page">

                <div className="loading-container">
                    <div className="spinner"></div>
                    <p>{t('loadingHotel')}</p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="hotel-details-page">

                <div className="error-container">
                    <h2>❌ {error}</h2>
                    <p>{t('hotelNotFoundText')}</p>
                    <button onClick={handleBackClick} className="btn-back">
                        ← {t('backToHotels')}
                    </button>
                </div>
            </div>
        );
    }

    if (!hotel) {
        return null;
    }

    const amenities = hotel.amenities ? JSON.parse(hotel.amenities) : [];
    const hotelImages = getHotelImages();

    return (
        <div className="hotel-details-page">


            {/* Hero Section */}
            <section
                className="hotel-hero"
                style={{ backgroundImage: `url(${hotelImages[currentImageIndex]})` }}
            >
                <div className="hero-overlay">
                    <div className="hero-actions">
                        <button onClick={handleBackClick} className="btn-back-hero">
                            ← {t('backToHotels')}
                        </button>
                        <button
                            className={`favorite-btn-hero ${isFavorite(parseInt(id)) ? 'favorited' : ''}`}
                            onClick={handleFavoriteClick}
                            aria-label={isFavorite(parseInt(id)) ? 'Remove from favorites' : 'Add to favorites'}
                        >
                            {isFavorite(parseInt(id)) ? '❤️' : '🤍'}
                        </button>
                    </div>
                    <div className="hotel-badges-hero">
                        <div className="star-rating-badge-hero">
                            {Array(hotel.starRating || 0).fill('★').join('')}
                        </div>
                        {hotel.rating > 0 && (
                            <div className="user-rating-badge-hero">
                                <span className="rating-value">{hotel.rating.toFixed(1)}</span>
                                <span className="rating-label">/ 5</span>
                            </div>
                        )}
                    </div>
                </div>

                {/* Image Navigation Controls */}
                {hotelImages.length > 1 && (
                    <>
                        <button className="image-nav-btn prev-btn" onClick={handlePreviousImage}>
                            ‹
                        </button>
                        <button className="image-nav-btn next-btn" onClick={handleNextImage}>
                            ›
                        </button>
                        <div className="image-counter">
                            {currentImageIndex + 1} / {hotelImages.length}
                        </div>
                    </>
                )}
            </section>

            {/* Thumbnail Gallery */}
            {hotelImages.length > 1 && (
                <div className="thumbnail-gallery container">
                    <div className="thumbnails-container">
                        {hotelImages.map((image, index) => (
                            <div
                                key={index}
                                className={`thumbnail ${index === currentImageIndex ? 'active' : ''}`}
                                onClick={() => handleThumbnailClick(index)}
                                style={{ backgroundImage: `url(${image})` }}
                            />
                        ))}
                    </div>
                </div>
            )}

            {/* Main Content */}
            <div className="hotel-details-content container">
                <div className="details-grid">
                    {/* Left Column - Main Info */}
                    <div className="details-main">
                        <div className="hotel-header">
                            <div>
                                <h1 className="hotel-name">{hotel.name}</h1>
                                <p className="hotel-location">
                                    📍 {hotel.location}, {hotel.city}, {hotel.country}
                                </p>
                            </div>
                            <div className="hotel-price-box">
                                <span className="price-label">{t('from')}</span>
                                <span className="price-amount">{convertAndFormat(hotel.displayPrice)}</span>
                                <span className="price-period">{t('perNight')}</span>
                            </div>
                        </div>

                        {hotel.description && (
                            <div className="hotel-description">
                                <h2>{t('aboutHotel')}</h2>
                                <p>{hotel.description}</p>
                            </div>
                        )}

                        {amenities.length > 0 && (
                            <div className="hotel-amenities-section">
                                <h2>{t('amenities')}</h2>
                                <div className="amenities-grid">
                                    {amenities.map((amenity, index) => (
                                        <div key={index} className="amenity-item">
                                            <span className="amenity-icon">✓</span>
                                            <span className="amenity-name">{amenity}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    {/* Right Column - Booking Widget */}
                    <div className="details-sidebar">
                        <BookingWidget
                            hotelId={parseInt(id)}
                            hotelName={hotel.name}
                            hotelImage={hotelImages[0]}
                            onBookingComplete={() => { }}
                        />

                        <div className="contact-card">
                            <h3>{t('needHelp')}</h3>
                            <p>{t('contactInfo')}</p>
                            <Link to={`/hotel/${id}/contact`} className="btn-contact">{t('contactSupport')}</Link>
                        </div>
                    </div>
                </div>

                {/* Admin Section - Room Management */}
                {isAdmin() && hotel.createdById === user?.id && (
                    <RoomTypeManager hotelId={parseInt(id)} />
                )}

                {/* Review Section */}
                <ReviewSection hotelId={parseInt(id)} />
            </div>
        </div>
    );
};

export default HotelDetails;
