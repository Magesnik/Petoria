import React, { useState, useEffect } from 'react';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import HotelCard from '../components/HotelCard';
import HotelFilters from '../components/HotelFilters';
import HotelMap from '../components/HotelMap';
import './Hotels.css';

const Hotels = () => {
    const { t } = useLanguage();
    const [hotels, setHotels] = useState([]);
    const [mapHotels, setMapHotels] = useState([]);
    const [cities, setCities] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [view, setView] = useState('grid'); // 'grid' or 'map'
    const [filters, setFilters] = useState({
        minPrice: '',
        maxPrice: '',
        city: '',
        amenities: [],
        minRating: null
    });

    // Fetch cities for filter dropdown
    useEffect(() => {
        fetchCities();
    }, []);

    // Fetch hotels when filters or search changes
    useEffect(() => {
        if (view === 'grid') {
            fetchHotels();
        } else {
            fetchHotelsForMap();
        }
    }, [filters, searchQuery, view]);

    const fetchCities = async () => {
        try {
            const response = await fetch('http://localhost:5150/api/hotels/cities');
            if (response.ok) {
                const data = await response.json();
                setCities(data);
            }
        } catch (err) {
            console.error('Error fetching cities:', err);
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
            if (filters.amenities.length > 0) params.append('amenities', filters.amenities.join(','));
            if (filters.minRating) params.append('minRating', filters.minRating);

            const response = await fetch(`http://localhost:5150/api/hotels?${params.toString()}`);

            if (!response.ok) {
                throw new Error('Failed to fetch hotels');
            }

            const data = await response.json();
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
            if (filters.amenities.length > 0) params.append('amenities', filters.amenities.join(','));
            if (filters.minRating) params.append('minRating', filters.minRating);

            const response = await fetch(`http://localhost:5150/api/hotels/map?${params.toString()}`);

            if (!response.ok) {
                throw new Error('Failed to fetch hotels for map');
            }

            const data = await response.json();
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
                    <h1>Find Your Perfect Stay</h1>
                    <p>Discover amazing hotels with the best amenities and prices</p>

                    <form className="search-box" onSubmit={handleSearch}>
                        <input
                            type="text"
                            className="search-input"
                            placeholder="Search by hotel name, city, or location..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                        />
                        <button type="submit" className="btn-search">
                            🔍 Search
                        </button>
                    </form>
                </div>
            </section>

            {/* Main Content */}
            <div className="hotels-container container">
                <aside className="filters-sidebar">
                    <HotelFilters
                        filters={filters}
                        onFilterChange={handleFilterChange}
                        cities={cities}
                        onClearFilters={handleClearFilters}
                    />
                </aside>

                <main className="hotels-main">
                    {/* Results Header with View Toggle */}
                    <div className="results-header">
                        <h2>
                            {loading ? 'Loading...' : view === 'grid'
                                ? `${hotels.length} Hotels Found`
                                : `${mapHotels.length} Hotels on Map`}
                        </h2>
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
                            <p>Loading hotels...</p>
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
                            <h3>No hotels found</h3>
                            <p>Try adjusting your filters or search criteria</p>
                            <button onClick={handleClearFilters} className="btn-clear">
                                Clear Filters
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
        </div>
    );
};

export default Hotels;
