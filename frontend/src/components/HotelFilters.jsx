import React from 'react';
import './HotelFilters.css';

const HotelFilters = ({ filters, onFilterChange, cities, onClearFilters }) => {
    const amenitiesList = ['WiFi', 'Pool', 'Parking', 'Gym', 'Restaurant', 'Pet-Friendly', 'Spa', 'Bar'];

    const handleAmenityToggle = (amenity) => {
        const currentAmenities = filters.amenities || [];
        const newAmenities = currentAmenities.includes(amenity)
            ? currentAmenities.filter(a => a !== amenity)
            : [...currentAmenities, amenity];
        onFilterChange('amenities', newAmenities);
    };

    return (
        <div className="hotel-filters">
            <div className="filters-header">
                <h3>Filters</h3>
                <button className="btn-clear-filters" onClick={onClearFilters}>
                    Clear All
                </button>
            </div>

            {/* Price Range */}
            <div className="filter-section">
                <label className="filter-label">Price Range (per night)</label>
                <div className="price-inputs">
                    <input
                        type="number"
                        placeholder="Min"
                        value={filters.minPrice || ''}
                        onChange={(e) => onFilterChange('minPrice', e.target.value)}
                        className="price-input"
                    />
                    <span>-</span>
                    <input
                        type="number"
                        placeholder="Max"
                        value={filters.maxPrice || ''}
                        onChange={(e) => onFilterChange('maxPrice', e.target.value)}
                        className="price-input"
                    />
                </div>
            </div>

            {/* City Filter */}
            <div className="filter-section">
                <label className="filter-label">City</label>
                <select
                    value={filters.city || ''}
                    onChange={(e) => onFilterChange('city', e.target.value)}
                    className="filter-select"
                >
                    <option value="">All Cities</option>
                    {cities.map((city, index) => (
                        <option key={index} value={city}>{city}</option>
                    ))}
                </select>
            </div>

            {/* Rating Filter */}
            <div className="filter-section">
                <label className="filter-label">Minimum Rating</label>
                <div className="rating-options">
                    {[5, 4, 3, 2, 1].map((rating) => (
                        <button
                            key={rating}
                            className={`rating-btn ${filters.minRating === rating ? 'active' : ''}`}
                            onClick={() => onFilterChange('minRating', filters.minRating === rating ? null : rating)}
                        >
                            {'★'.repeat(rating)}
                        </button>
                    ))}
                </div>
            </div>

            {/* Amenities Filter */}
            <div className="filter-section">
                <label className="filter-label">Amenities</label>
                <div className="amenities-list">
                    {amenitiesList.map((amenity) => (
                        <label key={amenity} className="amenity-checkbox">
                            <input
                                type="checkbox"
                                checked={(filters.amenities || []).includes(amenity)}
                                onChange={() => handleAmenityToggle(amenity)}
                            />
                            <span>{amenity}</span>
                        </label>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default HotelFilters;
