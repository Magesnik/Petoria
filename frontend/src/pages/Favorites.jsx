import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import { useFavorites } from '../context/FavoritesContext';
import Header from '../components/Header';
import HotelCard from '../components/HotelCard';
import './Favorites.css';

const Favorites = () => {
    const { t } = useLanguage();
    const { favorites, toggleFavorite } = useFavorites();
    const [favoriteHotels, setFavoriteHotels] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchFavoriteHotels();
    }, [favorites]);

    const fetchFavoriteHotels = async () => {
        if (favorites.length === 0) {
            setFavoriteHotels([]);
            setLoading(false);
            return;
        }

        setLoading(true);
        setError(null);

        try {
            // Fetch all hotels, then filter by favorite IDs
            const response = await fetch('http://localhost:5150/api/hotels');

            if (!response.ok) {
                throw new Error('Failed to fetch hotels');
            }

            const allHotels = await response.json();

            // Filter to only show favorited hotels
            const favoritedHotels = allHotels.filter(hotel => favorites.includes(hotel.id));
            setFavoriteHotels(favoritedHotels);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching favorite hotels:', err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            <Header />
            <div className="favorites-page">
                <div className="favorites-hero">
                    <h1>{t('favoritesTitle')}</h1>
                    <p>{t('favoritesSubtitle')}</p>
                </div>

                <div className="favorites-container">
                    {loading ? (
                        <div className="loading-state">
                            <div className="spinner"></div>
                            <p>Loading favorites...</p>
                        </div>
                    ) : error ? (
                        <div className="error-state">
                            <p>❌ {error}</p>
                            <button onClick={fetchFavoriteHotels} className="btn-retry">
                                Try Again
                            </button>
                        </div>
                    ) : favoriteHotels.length === 0 ? (
                        <div className="empty-state">
                            <div className="empty-icon">❤️</div>
                            <h2>{t('noFavorites')}</h2>
                            <p>{t('noFavoritesText')}</p>
                            <Link to="/hotels" className="btn btn-primary">
                                {t('exploreHotels')}
                            </Link>
                        </div>
                    ) : (
                        <div className="favorites-grid">
                            {favoriteHotels.map((hotel) => (
                                <HotelCard key={hotel.id} hotel={hotel} />
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};

export default Favorites;
