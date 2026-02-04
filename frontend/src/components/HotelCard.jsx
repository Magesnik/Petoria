import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useFavorites } from '../context/FavoritesContext';
import './HotelCard.css';

// Add these styles to HotelCard.css (simulated via instruction, user should check CSS file or I should edit it directly if I had read it. Since I didn't read it, I will assume it needs these classes. Wait, I should read it first or just overwrite/append? I'll append to the component file if using styled-components, but this is CSS import. I should edit the CSS file. I haven't read HotelCard.css yet. Let me read it first to be safe, but I'll skip that to speed up and assume standard modification. actually, I cannot edit styles in JSX. I must edit the CSS file. I will list_dir to find it.)
// Actually, I can just use inline styles or existing classes, but better to edit CSS.
// I will just queue the CSS edit after this.

const HotelCard = ({ hotel }) => {
    const navigate = useNavigate();
    const { isFavorite, toggleFavorite } = useFavorites();
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
                        <span className="price-amount">${hotel.pricePerNight}</span>
                        <span className="price-period">/night</span>
                    </div>
                    <button className="btn-view-details" onClick={handleViewDetails}>View Details</button>
                </div>
            </div>
        </div>
    );
};

export default HotelCard;
