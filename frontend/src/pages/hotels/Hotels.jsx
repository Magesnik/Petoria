import React, { useState, useEffect, useMemo } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { api } from '../../utils/api';
import { useLanguage } from '../../context/LanguageContext';

import HotelCard from '../../components/hotel/HotelCard';
import HotelFilters from '../../components/hotel/HotelFilters';
import './Hotels.css';

const HotelMap = React.lazy(() => import('../../components/hotel/HotelMap'));

const SORT_OPTIONS = [
    { value: 'rating_desc', labelKey: 'sortRatingDesc' },
    { value: 'rating_asc', labelKey: 'sortRatingAsc' },
    { value: 'name_asc', labelKey: 'sortNameAsc' },
    { value: 'name_desc', labelKey: 'sortNameDesc' },
    { value: 'price_asc', labelKey: 'sortPriceAsc' },
    { value: 'price_desc', labelKey: 'sortPriceDesc' },
    { value: 'stars_desc', labelKey: 'sortStarsDesc' },
    { value: 'stars_asc', labelKey: 'sortStarsAsc' },
    { value: 'amenities_desc', labelKey: 'sortAmenitiesDesc' },
    { value: 'amenities_asc', labelKey: 'sortAmenitiesAsc' },
    { value: 'availability_desc', labelKey: 'sortAvailabilityDesc' },
    { value: 'availability_asc', labelKey: 'sortAvailabilityAsc' },
    { value: 'discount_desc', labelKey: 'sortDiscountDesc' },
    { value: 'reviews_desc', labelKey: 'sortReviewsDesc' },
    { value: 'reviews_asc', labelKey: 'sortReviewsAsc' },
    { value: 'recent_reviews_desc', labelKey: 'sortRecentReviewsDesc' }
];

function sortHotels(hotels, sortBy) {
    const parseAmenities = (h) => {
        try { return JSON.parse(h.amenities || '[]').length; } catch { return 0; }
    };
    const arr = [...hotels];
    switch (sortBy) {
        case 'name_asc': return arr.sort((a, b) => a.name.localeCompare(b.name));
        case 'name_desc': return arr.sort((a, b) => b.name.localeCompare(a.name));
        case 'price_asc': return arr.sort((a, b) => (a.displayPrice || 0) - (b.displayPrice || 0));
        case 'price_desc': return arr.sort((a, b) => (b.displayPrice || 0) - (a.displayPrice || 0));
        case 'rating_desc': return arr.sort((a, b) => (b.rating || 0) - (a.rating || 0));
        case 'rating_asc': return arr.sort((a, b) => (a.rating || 0) - (b.rating || 0));
        case 'stars_desc': return arr.sort((a, b) => (b.starRating || 0) - (a.starRating || 0));
        case 'stars_asc': return arr.sort((a, b) => (a.starRating || 0) - (b.starRating || 0));
        case 'amenities_desc': return arr.sort((a, b) => parseAmenities(b) - parseAmenities(a));
        case 'amenities_asc': return arr.sort((a, b) => parseAmenities(a) - parseAmenities(b));
        case 'availability_desc': return arr.sort((a, b) => (b.availableRoomsTotal || 0) - (a.availableRoomsTotal || 0));
        case 'availability_asc': return arr.sort((a, b) => (a.availableRoomsTotal || 0) - (b.availableRoomsTotal || 0));
        case 'discount_desc': return arr.sort((a, b) => (b.discountPercentage || 0) - (a.discountPercentage || 0));
        case 'reviews_desc': return arr.sort((a, b) => (b.reviewCount || 0) - (a.reviewCount || 0));
        case 'reviews_asc': return arr.sort((a, b) => (a.reviewCount || 0) - (b.reviewCount || 0));
        case 'recent_reviews_desc': return arr.sort((a, b) => (b.recentReviewCount || 0) - (a.recentReviewCount || 0));
        default: return arr;
    }
}

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
    const location = useLocation();
    const navigate = useNavigate();
    const queryParams = useMemo(() => new URLSearchParams(location.search), [location.search]);
    const navState = location.state || {};

    const [searchQuery, setSearchQuery] = useState(() => {
        if (location.search || location.state) {
            return navState.search || queryParams.get('search') || '';
        }
        const saved = localStorage.getItem('petoria_hotel_filters');
        if (saved) {
            try { return JSON.parse(saved).searchQuery || ''; } catch { return ''; }
        }
        return '';
    });

    const [view, setView] = useState('grid'); // 'grid' or 'map'
    const [showMobileFilters, setShowMobileFilters] = useState(false);
    const [sortBy, setSortBy] = useState(() => {
        const saved = localStorage.getItem('petoria_hotel_filters');
        if (saved) {
            try { return JSON.parse(saved).sortBy || 'rating_desc'; } catch { return 'rating_desc'; }
        }
        return 'rating_desc';
    });

    const [filters, setFilters] = useState(() => {
        const defaultFilters = {
            minPrice: '', maxPrice: '', city: '', country: '', amenities: [],
            minRating: null, starRating: [], checkInDate: '', nights: '', guests: ''
        };

        if (location.search || location.state) {
            defaultFilters.country = navState.country || queryParams.get('country') || '';
            defaultFilters.amenities = navState.amenities ? (Array.isArray(navState.amenities) ? navState.amenities : [navState.amenities]) : (queryParams.get('amenities') ? queryParams.get('amenities').split(',') : []);
            defaultFilters.checkInDate = navState.checkInDate || queryParams.get('checkInDate') || '';
            defaultFilters.nights = navState.nights || (queryParams.get('nights') ? parseInt(queryParams.get('nights'), 10) : '');
            defaultFilters.guests = navState.guests || (queryParams.get('guests') ? parseInt(queryParams.get('guests'), 10) : '');
            return defaultFilters;
        }

        const saved = localStorage.getItem('petoria_hotel_filters');
        if (saved) {
            try {
                const parsed = JSON.parse(saved);
                if (parsed.filters) return { ...defaultFilters, ...parsed.filters };
            } catch { return defaultFilters; }
        }
        return defaultFilters;
    });

    const fetchFilterData = React.useCallback(async () => {
        try {
            // Fetch all filter data in parallel
            const [citiesData, countriesData, amenitiesData, priceRangeData] = await Promise.all([
                api.get('/hotels/cities'), // Ensure these API calls trigger correctly 
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
    }, []);

    const fetchHotels = React.useCallback(async () => {
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
            if (filters.starRating && filters.starRating.length > 0) params.append('starRating', filters.starRating.join(','));
            if (filters.checkInDate) params.append('checkInDate', filters.checkInDate);
            if (filters.nights) params.append('nights', filters.nights);
            if (filters.guests) params.append('guests', filters.guests);

            const data = await api.get(`/hotels?${params.toString()}`);
            setHotels(data);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching hotels:', err);
        } finally {
            setLoading(false);
        }
    }, [searchQuery, filters]);

    const fetchHotelsForMap = React.useCallback(async () => {
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
            if (filters.starRating && filters.starRating.length > 0) params.append('starRating', filters.starRating.join(','));

            const data = await api.get(`/hotels?${params.toString()}`);
            setMapHotels(data);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching map hotels:', err);
        } finally {
            setLoading(false);
        }
    }, [searchQuery, filters]);

    // Fetch filter data on mount
    useEffect(() => {
        fetchFilterData();

        // Clear history state and url parameters so a page refresh doesn't replay the search
        // and user doesn't see long URLs
        if (location.search || location.state) {
            navigate('/hotels', { replace: true });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [fetchFilterData, location.search, location.state, navigate]);

    // Save filters to localStorage whenever they change
    useEffect(() => {
        localStorage.setItem('petoria_hotel_filters', JSON.stringify({
            searchQuery,
            filters,
            sortBy
        }));
    }, [searchQuery, filters, sortBy]);

    // Fetch hotels when filters or search changes
    useEffect(() => {
        if (view === 'grid') {
            fetchHotels();
        } else {
            fetchHotelsForMap();
        }
    }, [filters, searchQuery, view, fetchHotels, fetchHotelsForMap]);

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
            minRating: null,
            starRating: [],
            checkInDate: '',
            nights: '',
            guests: ''
        });
        setSearchQuery('');
        localStorage.removeItem('petoria_hotel_filters');
    };

    const handleSearch = (e) => {
        e.preventDefault();
    };

    const sortedHotels = useMemo(() => sortHotels(hotels, sortBy), [hotels, sortBy]);

    return (
        <div className="hotels-page">


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
                        <div className="sort-and-view">
                            {view === 'grid' && (
                                <div className="sort-toolbar">
                                    <label className="sort-label">{t('sortByWithIcon')}</label>
                                    <select
                                        className="sort-select"
                                        value={sortBy}
                                        onChange={(e) => setSortBy(e.target.value)}
                                    >
                                        {SORT_OPTIONS.map(opt => (
                                            <option key={opt.value} value={opt.value}>{t(opt.labelKey)}</option>
                                        ))}
                                    </select>
                                </div>
                            )}
                            <div className="view-toggle">
                                <button
                                    className={`view-btn ${view === 'grid' ? 'active' : ''}`}
                                    onClick={() => setView('grid')}
                                    title={t('gridView')}
                                >
                                    ⊞ {t('grid')}
                                </button>
                                <button
                                    className={`view-btn ${view === 'map' ? 'active' : ''}`}
                                    onClick={() => setView('map')}
                                    title={t('mapView')}
                                >
                                    🗺️ {t('map')}
                                </button>
                            </div>
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
                                {t('tryAgain')}
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
                        <React.Suspense fallback={<div className="loading-state"><div className="spinner"></div></div>}>
                            <HotelMap hotels={mapHotels} />
                        </React.Suspense>
                    )}

                    {/* Hotels Grid */}
                    {!loading && !error && view === 'grid' && sortedHotels.length > 0 && (
                        <div className="hotels-grid">
                            {sortedHotels.map((hotel) => (
                                <HotelCard key={hotel.id} hotel={hotel} />
                            ))}
                        </div>
                    )}
                </main>
            </div>
        </div>
    );
};

export default Hotels;
