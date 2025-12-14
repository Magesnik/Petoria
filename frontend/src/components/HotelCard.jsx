import React from 'react';
import { useNavigate } from 'react-router-dom';
import './HotelCard.css';

const HotelCard = ({ hotel }) => {
    const navigate = useNavigate();
    const amenities = hotel.amenities ? JSON.parse(hotel.amenities) : [];
    const displayAmenities = amenities.slice(0, 4);

    const handleViewDetails = () => {
        navigate(`/hotels/${hotel.id}`);
    };

    return (
        <div className="hotel-card">
            <div
                className="hotel-card-image"
                style={{ backgroundImage: `url(${hotel.imageUrl || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80'})` }}
            >
                {hotel.rating > 0 && (
                    <div className="hotel-rating">
                        <span className="rating-star">★</span>
                        <span className="rating-value">{hotel.rating.toFixed(1)}</span>
                    </div>
                )}
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
