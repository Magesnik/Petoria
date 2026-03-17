import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../../context/LanguageContext';
import { useFavorites } from '../../context/FavoritesContext';
import { useAuth } from '../../context/AuthContext';

import HotelCard from '../../components/hotel/HotelCard';
import './Favorites.css';

/** Страница с любими хотели на потребителя, зредена от FavoritesContext. */
const Favorites = () => {
    const { t } = useLanguage();
    const { favorites, getFavoritesWithDetails, loading: favoritesLoading } = useFavorites();
    const { user } = useAuth();
    // Детайлни данни за любимите хотели (от API)
    const [favoriteHotels, setFavoriteHotels] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchFavoriteHotels = React.useCallback(async () => {
        if (!user) {
            setFavoriteHotels([]);
            setLoading(false);
            return;
        }

        if (favorites.length === 0 && !favoritesLoading) {
            setFavoriteHotels([]);
            setLoading(false);
            return;
        }

        setLoading(true);
        setError(null);

        try {
            // Use the API to get favorites with full hotel details
            const favoritesData = await getFavoritesWithDetails();

            // Transform API response to match HotelCard expected format
            const hotels = favoritesData.map(fav => ({
                id: fav.hotelId,
                name: fav.hotelName,
                city: fav.hotelCity,
                country: fav.hotelCountry,
                imageUrl: fav.hotelImageUrl,
                pricePerNight: fav.hotelPricePerNight,
                rating: fav.hotelRating
            }));

            setFavoriteHotels(hotels);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching favorite hotels:', err);
        } finally {
            setLoading(false);
        }
    }, [user, favorites.length, favoritesLoading, getFavoritesWithDetails]);

    useEffect(() => {
        fetchFavoriteHotels();
    }, [fetchFavoriteHotels]);

    return (
        <>

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
