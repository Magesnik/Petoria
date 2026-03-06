import React, { useState, useMemo, useEffect } from 'react';
import { useLanguage } from '../context/LanguageContext';
import './HotelFilters.css';

const HotelFilters = ({
    filters,
    onFilterChange,
    cities,
    countries,
    allAmenities,
    priceRange,
    onClearFilters
}) => {
    const { t } = useLanguage();
    const [citySearch, setCitySearch] = useState('');
    const [countrySearch, setCountrySearch] = useState('');
    const [amenitySearch, setAmenitySearch] = useState('');
    const [showCityDropdown, setShowCityDropdown] = useState(false);
    const [showCountryDropdown, setShowCountryDropdown] = useState(false);

    // Filter cities based on search
    const filteredCities = useMemo(() => {
        if (!citySearch) return cities;
        return cities.filter(city =>
            city.toLowerCase().includes(citySearch.toLowerCase())
        );
    }, [cities, citySearch]);

    // Filter countries based on search
    const filteredCountries = useMemo(() => {
        if (!countrySearch) return countries;
        return countries.filter(country =>
            country.toLowerCase().includes(countrySearch.toLowerCase())
        );
    }, [countries, countrySearch]);

    // Filter amenities based on search
    const filteredAmenities = useMemo(() => {
        if (!amenitySearch) return allAmenities;
        return allAmenities.filter(amenity =>
            amenity.toLowerCase().includes(amenitySearch.toLowerCase())
        );
    }, [allAmenities, amenitySearch]);

    const handleAmenityToggle = (amenity) => {
        const currentAmenities = filters.amenities || [];
        const newAmenities = currentAmenities.includes(amenity)
            ? currentAmenities.filter(a => a !== amenity)
            : [...currentAmenities, amenity];
        onFilterChange('amenities', newAmenities);
    };

    const handleStarRatingToggle = (rating) => {
        const currentStars = filters.starRating || [];
        const newStars = currentStars.includes(rating)
            ? currentStars.filter(r => r !== rating)
            : [...currentStars, rating];
        onFilterChange('starRating', newStars);
    };

    const handleCitySelect = (city) => {
        onFilterChange('city', city);
        setCitySearch(city);
        setShowCityDropdown(false);
    };

    const handleCountrySelect = (country) => {
        onFilterChange('country', country);
        setCountrySearch(country);
        setShowCountryDropdown(false);
    };

    const handleClearCity = () => {
        onFilterChange('city', '');
        setCitySearch('');
    };

    const handleClearCountry = () => {
        onFilterChange('country', '');
        setCountrySearch('');
    };

    // Price slider values
    // Use local state for immediate UI feedback, debounce updates to parent
    const [localMinPrice, setLocalMinPrice] = useState(filters.minPrice || priceRange.minPrice);
    const [localMaxPrice, setLocalMaxPrice] = useState(filters.maxPrice || Math.ceil(priceRange.maxPrice));

    // Update local state when props change (e.g. initial load or clear filters)
    useEffect(() => {
        setLocalMinPrice(filters.minPrice !== '' ? filters.minPrice : priceRange.minPrice);
        setLocalMaxPrice(filters.maxPrice !== '' ? filters.maxPrice : Math.ceil(priceRange.maxPrice));
    }, [filters.minPrice, filters.maxPrice, priceRange]);

    // Debounce updates to parent
    useEffect(() => {
        const timer = setTimeout(() => {
            const effectiveMin = filters.minPrice !== '' ? filters.minPrice : priceRange.minPrice;
            const effectiveMax = filters.maxPrice !== '' ? filters.maxPrice : Math.ceil(priceRange.maxPrice);

            if (localMinPrice !== effectiveMin) {
                onFilterChange('minPrice', localMinPrice);
            }

            if (localMaxPrice !== effectiveMax) {
                onFilterChange('maxPrice', localMaxPrice);
            }
        }, 500); // 500ms debounce

        return () => clearTimeout(timer);
    }, [localMinPrice, localMaxPrice, filters.minPrice, filters.maxPrice, priceRange.minPrice, priceRange.maxPrice]);

    const rangeMax = Math.ceil(priceRange.maxPrice);

    return (
        <div className="hotel-filters">
            <div className="filters-header">
                <h3>{t('filters')}</h3>
                <button className="btn-clear-filters" onClick={onClearFilters}>
                    {t('clearAll')}
                </button>
            </div>

            {/* Dates & Guests Filter */}
            <div className="filter-section">
                <label className="filter-label">{t('accommodationAndGuests')}</label>

                <div style={{ marginBottom: '10px' }}>
                    <label style={{ fontSize: '0.8rem', color: '#666', display: 'block', marginBottom: '4px' }}>{t('checkInDate')}</label>
                    <input
                        type="date"
                        className="filter-search-input"
                        value={filters.checkInDate || ''}
                        onChange={(e) => onFilterChange('checkInDate', e.target.value)}
                        min={new Date().toISOString().split('T')[0]}
                    />
                </div>

                <div style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
                    <div style={{ flex: 1 }}>
                        <label style={{ fontSize: '0.8rem', color: '#666', display: 'block', marginBottom: '4px' }}>{t('nightsLabel')}</label>
                        <input
                            type="number"
                            className="filter-search-input"
                            value={filters.nights || ''}
                            onChange={(e) => onFilterChange('nights', parseInt(e.target.value) || '')}
                            min="1"
                        />
                    </div>
                    <div style={{ flex: 1 }}>
                        <label style={{ fontSize: '0.8rem', color: '#666', display: 'block', marginBottom: '4px' }}>{t('guestsLabel')}</label>
                        <input
                            type="number"
                            className="filter-search-input"
                            value={filters.guests || ''}
                            onChange={(e) => onFilterChange('guests', parseInt(e.target.value) || '')}
                            min="1"
                        />
                    </div>
                </div>
            </div>

            {/* Price Range Slider */}
            <div className="filter-section">
                <label className="filter-label">{t('priceRangePerNight')}</label>
                <div className="price-slider-container">
                    {/* Editable Price Inputs */}
                    <div className="price-inputs-row">
                        <div className="price-input-group">
                            <span className="currency-symbol">$</span>
                            <input
                                type="number"
                                className="price-input-field"
                                value={localMinPrice}
                                min={priceRange.minPrice}
                                max={localMaxPrice - 1}
                                onChange={(e) => {
                                    const value = parseInt(e.target.value) || priceRange.minPrice;
                                    if (value < localMaxPrice) {
                                        setLocalMinPrice(value);
                                    }
                                }}
                            />
                        </div>
                        <span className="price-separator">—</span>
                        <div className="price-input-group">
                            <span className="currency-symbol">$</span>
                            <input
                                type="number"
                                className="price-input-field"
                                value={localMaxPrice}
                                min={localMinPrice + 1}
                                max={rangeMax}
                                onChange={(e) => {
                                    const value = parseInt(e.target.value) || rangeMax;
                                    if (value > localMinPrice) {
                                        setLocalMaxPrice(value);
                                    }
                                }}
                            />
                        </div>
                    </div>

                    {/* Dual Range Slider */}
                    <div className="slider-container">
                        <div className="slider-track-bg"></div>
                        <div
                            className="slider-track-fill"
                            style={{
                                left: `${((localMinPrice - priceRange.minPrice) / (rangeMax - priceRange.minPrice)) * 100}%`,
                                width: `${((localMaxPrice - localMinPrice) / (rangeMax - priceRange.minPrice)) * 100}%`
                            }}
                        />
                        <input
                            type="range"
                            className="slider slider-min"
                            min={priceRange.minPrice}
                            max={rangeMax}
                            value={localMinPrice}
                            onChange={(e) => {
                                const value = Math.min(parseInt(e.target.value), localMaxPrice - 1);
                                setLocalMinPrice(value);
                            }}
                        />
                        <input
                            type="range"
                            className="slider slider-max"
                            min={priceRange.minPrice}
                            max={rangeMax}
                            value={localMaxPrice}
                            onChange={(e) => {
                                const value = Math.max(parseInt(e.target.value), localMinPrice + 1);
                                setLocalMaxPrice(value);
                            }}
                        />
                    </div>
                </div>
            </div>

            {/* Country Filter with Search */}
            <div className="filter-section">
                <label className="filter-label">{t('country')}</label>
                <div className="searchable-select">
                    <div className="search-input-wrapper">
                        <input
                            type="text"
                            className="filter-search-input"
                            placeholder={t('searchCountries')}
                            value={countrySearch}
                            onChange={(e) => {
                                setCountrySearch(e.target.value);
                                if (!e.target.value) {
                                    onFilterChange('country', '');
                                }
                            }}
                            onFocus={() => setShowCountryDropdown(true)}
                            onBlur={() => setTimeout(() => setShowCountryDropdown(false), 200)}
                        />
                        {filters.country && (
                            <button className="clear-selection" onClick={handleClearCountry}>×</button>
                        )}
                    </div>
                    {showCountryDropdown && filteredCountries.length > 0 && (
                        <div className="dropdown-list">
                            <div
                                className={`dropdown-item ${!filters.country ? 'active' : ''}`}
                                onClick={() => handleCountrySelect('')}
                            >
                                {t('allCountries')}
                            </div>
                            {filteredCountries.map((country, index) => (
                                <div
                                    key={index}
                                    className={`dropdown-item ${filters.country === country ? 'active' : ''}`}
                                    onClick={() => handleCountrySelect(country)}
                                >
                                    {country}
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            {/* City Filter with Search */}
            <div className="filter-section">
                <label className="filter-label">{t('city')}</label>
                <div className="searchable-select">
                    <div className="search-input-wrapper">
                        <input
                            type="text"
                            className="filter-search-input"
                            placeholder={t('searchCities')}
                            value={citySearch}
                            onChange={(e) => {
                                setCitySearch(e.target.value);
                                if (!e.target.value) {
                                    onFilterChange('city', '');
                                }
                            }}
                            onFocus={() => setShowCityDropdown(true)}
                            onBlur={() => setTimeout(() => setShowCityDropdown(false), 200)}
                        />
                        {filters.city && (
                            <button className="clear-selection" onClick={handleClearCity}>×</button>
                        )}
                    </div>
                    {showCityDropdown && filteredCities.length > 0 && (
                        <div className="dropdown-list">
                            <div
                                className={`dropdown-item ${!filters.city ? 'active' : ''}`}
                                onClick={() => handleCitySelect('')}
                            >
                                {t('allCities')}
                            </div>
                            {filteredCities.map((city, index) => (
                                <div
                                    key={index}
                                    className={`dropdown-item ${filters.city === city ? 'active' : ''}`}
                                    onClick={() => handleCitySelect(city)}
                                >
                                    {city}
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            {/* Star Rating Filter (Hotel Class) */}
            <div className="filter-section">
                <label className="filter-label">{t('hotelClass')}</label>
                <div className="star-rating-select-filter">
                    {[1, 2, 3, 4, 5].map((star) => (
                        <button
                            key={star}
                            className={`star-select-btn-filter ${(filters.starRating || []).includes(star) ? 'selected' : ''}`}
                            onClick={() => handleStarRatingToggle(star)}
                            title={`${star} ${star === 1 ? t('star') : t('starsCount')}`}
                        >
                            <span className="star-icon">★</span>
                            <span className="star-number">{star}</span>
                        </button>
                    ))}
                </div>
            </div>

            {/* Min Rating Filter (User Reviews) */}
            <div className="filter-section">
                <label className="filter-label">{t('guestRating')}</label>
                <div className="rating-options-grid">
                    {[
                        { value: 4.5, label: '4.5+' },
                        { value: 4.0, label: '4.0+' },
                        { value: 3.5, label: '3.5+' },
                        { value: 3.0, label: '3.0+' }
                    ].map((option) => (
                        <button
                            key={option.value}
                            className={`rating-pill ${filters.minRating === option.value ? 'active' : ''}`}
                            onClick={() => onFilterChange('minRating', filters.minRating === option.value ? null : option.value)}
                        >
                            <span className="rating-score">{option.label}</span>
                            <span className="rating-desc">{option.value >= 4.5 ? t('excellent') : option.value >= 4 ? t('veryGood') : t('good')}</span>
                        </button>
                    ))}
                </div>
            </div>

            {/* Amenities Filter with Search */}
            <div className="filter-section">
                <label className="filter-label">
                    {t('amenities')}
                    {filters.amenities?.length > 0 && (
                        <span className="selected-count">{t('selectedCount').replace('{count}', filters.amenities.length)}</span>
                    )}
                </label>
                <div className="amenities-search">
                    <input
                        type="text"
                        className="filter-search-input"
                        placeholder={t('searchAmenities')}
                        value={amenitySearch}
                        onChange={(e) => setAmenitySearch(e.target.value)}
                    />
                </div>
                <div className="amenities-list">
                    {filteredAmenities.length > 0 ? (
                        filteredAmenities.map((amenity) => (
                            <label key={amenity} className="amenity-checkbox">
                                <input
                                    type="checkbox"
                                    checked={(filters.amenities || []).includes(amenity)}
                                    onChange={() => handleAmenityToggle(amenity)}
                                />
                                <span>{amenity}</span>
                            </label>
                        ))
                    ) : (
                        <p className="no-results">{t('noAmenitiesFound')}</p>
                    )}
                </div>
            </div>
        </div>
    );
};

export default HotelFilters;
