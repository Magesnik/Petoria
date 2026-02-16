import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import Footer from '../components/Footer';
import HotelCard from '../components/HotelCard';
import HotelFilters from '../components/HotelFilters';
import HotelMap from '../components/HotelMap';
import './Hotels.css';

const Hotels = () => {
    const { t } = useLanguage();
    const [hotels, setHotels] = useState([]);
    const [mapHotels, setMapHotels] = useState([]);
    const [cities, setCities] = useState([]);
    const [countries, setCountries] = useState([]);
    const [allAmenities, setAllAmenities] = useState([]);
    const [priceRange, setPriceRange] = useState({ minPrice: 0, maxPrice: 1000 });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [view, setView] = useState('grid'); // 'grid' or 'map'
    const [showMobileFilters, setShowMobileFilters] = useState(false);
    const [filters, setFilters] = useState({
        minPrice: '',
        maxPrice: '',
        city: '',
        country: '',
        amenities: [],
        minRating: null
    });

    // Fetch filter data on mount
    useEffect(() => {
        fetchFilterData();
    }, []);

    // Fetch hotels when filters or search changes
    useEffect(() => {
        if (view === 'grid') {
            fetchHotels();
        } else {
            fetchHotelsForMap();
        }
    }, [filters, searchQuery, view]);

    const fetchFilterData = async () => {
        try {
            // Fetch all filter data in parallel
            const [citiesData, countriesData, amenitiesData, priceRangeData] = await Promise.all([
                api.get('/hotels/cities'),
                api.get('/hotels/countries'),
                api.get('/hotels/amenities'),
                api.get('/hotels/price-range')
            ]);

            setCities(citiesData);
            setCountries(countriesData);
            setAllAmenities(amenitiesData);
            setPriceRange(priceRangeData);
        } catch (err) {
            console.error('Error fetching filter data:', err);
        }
    };

    const fetchHotels = async () => {
        setLoading(true);
        setError(null);

        try {
            // Build query parameters
            const params = new URLSearchParams();

            if (searchQuery) params.append('search', searchQuery);
            if (filters.minPrice) params.append('minPrice', filters.minPrice);
            if (filters.maxPrice) params.append('maxPrice', filters.maxPrice);
            if (filters.city) params.append('city', filters.city);
            if (filters.country) params.append('country', filters.country);
            if (filters.amenities.length > 0) params.append('amenities', filters.amenities.join(','));
            if (filters.minRating) params.append('minRating', filters.minRating);

            const data = await api.get(`/hotels?${params.toString()}`);
            setHotels(data);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching hotels:', err);
        } finally {
            setLoading(false);
        }
    };

    const fetchHotelsForMap = async () => {
        setLoading(true);
        setError(null);

        try {
            // Build query parameters
            const params = new URLSearchParams();

            if (searchQuery) params.append('search', searchQuery);
            if (filters.minPrice) params.append('minPrice', filters.minPrice);
            if (filters.maxPrice) params.append('maxPrice', filters.maxPrice);
            if (filters.city) params.append('city', filters.city);
            if (filters.country) params.append('country', filters.country);
            if (filters.amenities.length > 0) params.append('amenities', filters.amenities.join(','));
            if (filters.minRating) params.append('minRating', filters.minRating);

            const data = await api.get(`/hotels/map?${params.toString()}`);
            setMapHotels(data);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching hotels for map:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleFilterChange = (filterName, value) => {
        setFilters(prev => ({
            ...prev,
            [filterName]: value
        }));
    };

    const handleClearFilters = () => {
        setFilters({
            minPrice: '',
            maxPrice: '',
            city: '',
            country: '',
            amenities: [],
            minRating: null
        });
        setSearchQuery('');
    };

    const handleSearch = (e) => {
        e.preventDefault();
    };

    return (
        <div className="hotels-page">
            <Header />

            {/* Hero Section with Search */}
            <section className="hotels-hero">
                <div className="hero-content">
                    <h1>{t('findPerfectStay')}</h1>
                    <p>{t('discoverAmazing')}</p>

                    <form className="search-box" onSubmit={handleSearch}>
                        <input
                            type="text"
                            className="search-input"
                            placeholder={t('searchPlaceholder')}
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                        />
                        <button type="submit" className="btn-search">
                            🔍 {t('search')}
                        </button>
                    </form>
                </div>
            </section>

            {/* Main Content */}
            <div className="hotels-container">
                <div
                    className={`mobile-overlay ${showMobileFilters ? 'visible' : ''}`}
                    onClick={() => setShowMobileFilters(false)}
                ></div>
                <aside className={`filters-sidebar ${showMobileFilters ? 'open' : ''}`}>
                    <div className="mobile-filter-header">
                        <h3>{t('filters')}</h3>
                        <button className="btn-close-filters" onClick={() => setShowMobileFilters(false)}>✕</button>
                    </div>
                    <HotelFilters
                        filters={filters}
                        onFilterChange={handleFilterChange}
                        cities={cities}
                        countries={countries}
                        allAmenities={allAmenities}
                        priceRange={priceRange}
                        onClearFilters={handleClearFilters}
                    />
                </aside>

                <main className="hotels-main">
                    {/* Results Header with View Toggle */}
                    <div className="results-header">
                        <div className="header-left">
                            <h2>
                                {loading ? t('loading') : view === 'grid'
                                    ? `${hotels.length} ${t('hotelsFound')}`
                                    : `${mapHotels.length} ${t('hotelsOnMap')}`}
                            </h2>
                            <button
                                className="btn-filter-toggle"
                                onClick={() => setShowMobileFilters(true)}
                            >
                                <span className="filter-icon">☰</span> {t('filters')}
                            </button>
                        </div>
                        <div className="view-toggle">
                            <button
                                className={`view-btn ${view === 'grid' ? 'active' : ''}`}
                                onClick={() => setView('grid')}
                                title="Grid View"
                            >
                                ⊞ Grid
                            </button>
                            <button
                                className={`view-btn ${view === 'map' ? 'active' : ''}`}
                                onClick={() => setView('map')}
                                title="Map View"
                            >
                                🗺️ Map
                            </button>
                        </div>
                    </div>

                    {/* Loading State */}
                    {loading && (
                        <div className="loading-state">
                            <div className="spinner"></div>
                            <p>{t('loading')}</p>
                        </div>
                    )}

                    {/* Error State */}
                    {error && (
                        <div className="error-state">
                            <p>❌ {error}</p>
                            <button onClick={view === 'grid' ? fetchHotels : fetchHotelsForMap} className="btn-retry">
                                Try Again
                            </button>
                        </div>
                    )}

                    {/* Empty State */}
                    {!loading && !error && view === 'grid' && hotels.length === 0 && (
                        <div className="empty-state">
                            <h3>{t('noHotelsFound')}</h3>
                            <p>{t('tryAdjustingFilters')}</p>
                            <button onClick={handleClearFilters} className="btn-clear">
                                {t('clearFilters')}
                            </button>
                        </div>
                    )}

                    {/* Map View */}
                    {!loading && !error && view === 'map' && (
                        <HotelMap hotels={mapHotels} />
                    )}

                    {/* Hotels Grid */}
                    {!loading && !error && view === 'grid' && hotels.length > 0 && (
                        <div className="hotels-grid">
                            {hotels.map((hotel) => (
                                <HotelCard key={hotel.id} hotel={hotel} />
                            ))}
                        </div>
                    )}
                </main>
            </div>

            <Footer />
        </div>
    );
};

export default Hotels;
