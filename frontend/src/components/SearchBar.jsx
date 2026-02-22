import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';
import './SearchBar.css';

const SearchBar = ({ initialValues = {} }) => {
    const navigate = useNavigate();
    const { t } = useLanguage();

    const [countries, setCountries] = useState([]);
    const [allAmenities, setAllAmenities] = useState([]);

    const [searchQuery, setSearchQuery] = useState(initialValues.searchQuery || '');
    const [country, setCountry] = useState(initialValues.country || '');
    const [amenities, setAmenities] = useState(
        initialValues.amenities && Array.isArray(initialValues.amenities)
            ? initialValues.amenities
            : (initialValues.amenity ? [initialValues.amenity] : [])
    );
    const [isAmenitiesOpen, setIsAmenitiesOpen] = useState(false);

    // Close the custom dropdown when clicking outside
    useEffect(() => {
        const handleClickOutside = (e) => {
            if (!e.target.closest('.amenities-dropdown-container')) {
                setIsAmenitiesOpen(false);
            }
        };
        document.addEventListener('click', handleClickOutside);
        return () => document.removeEventListener('click', handleClickOutside);
    }, []);

    const [checkInDate, setCheckInDate] = useState(initialValues.checkInDate || '');
    const [nights, setNights] = useState(initialValues.nights || 1);
    const [guests, setGuests] = useState(initialValues.guests || 2);

    useEffect(() => {
        const fetchFilterData = async () => {
            try {
                const [countriesData, amenitiesData] = await Promise.all([
                    api.get('/hotels/countries'),
                    api.get('/hotels/amenities')
                ]);
                setCountries(countriesData);
                setAllAmenities(amenitiesData);
            } catch (err) {
                console.error('Error fetching search filter data:', err);
            }
        };

        fetchFilterData();
    }, []);

    const handleSubmit = (e) => {
        e.preventDefault();

        const stateParams = {};
        if (searchQuery) stateParams.search = searchQuery;
        if (country) stateParams.country = country;
        if (amenities.length > 0) stateParams.amenities = amenities;
        if (checkInDate) stateParams.checkInDate = checkInDate;
        if (nights) stateParams.nights = nights;
        if (guests) stateParams.guests = guests;

        navigate('/hotels', { state: stateParams });
    };

    return (
        <form className="advanced-search-bar" onSubmit={handleSubmit}>
            <div className="search-field query-field">
                <span className="field-icon">🏨</span>
                <div className="field-content">
                    <label>{t('sbHotelName')}</label>
                    <input
                        type="text"
                        placeholder={t('sbFindHotel')}
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>
            </div>

            <div className="search-divider"></div>

            <div className="search-field select-field">
                <span className="field-icon">📍</span>
                <div className="field-content">
                    <label>{t('sbCountry')}</label>
                    <select value={country} onChange={(e) => setCountry(e.target.value)}>
                        <option value="">{t('sbPleaseSelect')}</option>
                        {countries.map(c => (
                            <option key={c} value={c}>{c}</option>
                        ))}
                    </select>
                </div>
            </div>

            <div className="search-divider"></div>

            <div className="search-field select-field amenities-dropdown-container">
                <span className="field-icon">🎱</span>
                <div className="field-content" onClick={() => setIsAmenitiesOpen(!isAmenitiesOpen)} style={{ cursor: 'pointer' }}>
                    <label>{t('sbAmenity')}</label>
                    <div className="custom-select-trigger">
                        {amenities.length === 0
                            ? <span className="placeholder-text">{t('sbPleaseSelect')}</span>
                            : <span className="selected-text">{amenities.length} {t('items') || 'избрани'}</span>
                        }
                    </div>
                    {isAmenitiesOpen && (
                        <div className="custom-dropdown-menu" onClick={e => e.stopPropagation()}>
                            {allAmenities.map(a => (
                                <label key={a} className="dropdown-item">
                                    <input
                                        type="checkbox"
                                        checked={amenities.includes(a)}
                                        onChange={(e) => {
                                            if (e.target.checked) {
                                                setAmenities([...amenities, a]);
                                            } else {
                                                setAmenities(amenities.filter(item => item !== a));
                                            }
                                        }}
                                    />
                                    {a}
                                </label>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <div className="search-divider"></div>

            <div className="search-field date-field">
                <span className="field-icon">📅</span>
                <div className="field-content">
                    <label>{t('sbCheckIn')}</label>
                    <input
                        type="date"
                        value={checkInDate}
                        onChange={(e) => setCheckInDate(e.target.value)}
                        min={new Date().toISOString().split('T')[0]}
                    />
                </div>
            </div>

            <div className="search-divider"></div>

            <div className="search-field number-field">
                <span className="field-icon">🌙</span>
                <div className="field-content">
                    <label>{t('sbNights')}</label>
                    <div className="number-controls">
                        <button type="button" onClick={() => setNights(Math.max(1, nights - 1))}>-</button>
                        <span>{nights}</span>
                        <button type="button" onClick={() => setNights(nights + 1)}>+</button>
                    </div>
                </div>
            </div>

            <div className="search-divider"></div>

            <div className="search-field guests-field">
                <span className="field-icon">👤</span>
                <div className="field-content">
                    <label>{t('sbGuests')}</label>
                    <div className="number-controls">
                        <button type="button" onClick={() => setGuests(Math.max(1, guests - 1))}>-</button>
                        <span>{guests}</span>
                        <button type="button" onClick={() => setGuests(guests + 1)}>+</button>
                    </div>
                </div>
            </div>

            <button type="submit" className="btn-advanced-search">
                {t('sbSearch')}
            </button>
        </form>
    );
};

export default SearchBar;
