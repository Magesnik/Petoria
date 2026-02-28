import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useFavorites } from '../context/FavoritesContext';
import { useCurrency } from '../context/CurrencyContext';
import { useLanguage } from '../context/LanguageContext';
import './HotelCard.css';


const HotelCard = ({ hotel }) => {
    const navigate = useNavigate();
    const { isFavorite, toggleFavorite } = useFavorites();
    const { convertAndFormat } = useCurrency();
    const { t } = useLanguage();
    const amenities = hotel.amenities ? JSON.parse(hotel.amenities) : [];
    const displayAmenities = amenities.slice(0, 4);

    const handleViewDetails = () => {
        navigate(`/hotels/${hotel.id}`);
    };

    const handleFavoriteClick = (e) => {
        e.stopPropagation(); // Prevent card navigation
        toggleFavorite(hotel.id);
    };

    return (
        <div className="hotel-card">
            <div
                className="hotel-card-image"
                style={{ backgroundImage: `url(${hotel.imageUrl || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80'})` }}
            >
                <button
                    className={`favorite-btn ${isFavorite(hotel.id) ? 'favorited' : ''}`}
                    onClick={handleFavoriteClick}
                    aria-label={isFavorite(hotel.id) ? 'Remove from favorites' : 'Add to favorites'}
                >
                    {isFavorite(hotel.id) ? '❤️' : '🤍'}
                </button>
                <div className="hotel-badges">
                    {hotel.hasDiscount && hotel.discountPercentage && (
                        <div className="discount-badge-card">
                            -{hotel.discountPercentage}%
                        </div>
                    )}
                    <div className="star-rating-badge">
                        {Array(hotel.starRating || 0).fill('★').join('')}
                    </div>
                    {hotel.rating > 0 && (
                        <div className="user-rating-badge">
                            <span className="rating-value">{hotel.rating.toFixed(1)}</span>
                            <span className="rating-label">/ 5</span>
                        </div>
                    )}
                </div>
            </div>
            <div className="hotel-card-content">
                <h3 className="hotel-name">{hotel.name}</h3>
                <p className="hotel-location">📍 {hotel.city}, {hotel.country}</p>

                {displayAmenities.length > 0 && (
                    <div className="hotel-amenities">
                        {displayAmenities.map((amenity, index) => (
                            <span key={index} className="amenity-tag">{amenity}</span>
                        ))}
                        {amenities.length > 4 && (
                            <span className="amenity-tag">+{amenities.length - 4}</span>
                        )}
                    </div>
                )}

                <div className="hotel-footer">
                    <div className="hotel-price">
                        {hotel.hasDiscount ? (
                            <>
                                <div className="price-with-discount">
                                    <span className="price-original">{convertAndFormat(hotel.originalPrice)}</span>
                                    <span className="price-amount">{convertAndFormat(hotel.displayPrice)}</span>
                                </div>
                                <span className="price-period">{t('perNight')}</span>
                            </>
                        ) : (
                            <>
                                <span className="price-amount">{convertAndFormat(hotel.displayPrice || hotel.pricePerNight)}</span>
                                <span className="price-period">{t('perNight')}</span>
                            </>
                        )}
                    </div>
                    <button className="btn-view-details" onClick={handleViewDetails}>{t('viewDetails')}</button>
                </div>
            </div>
        </div>
    );
};

export default HotelCard;
