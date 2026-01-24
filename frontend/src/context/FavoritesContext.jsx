import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { useAuth } from './AuthContext';

const FavoritesContext = createContext();

export const useFavorites = () => {
    const context = useContext(FavoritesContext);
    if (!context) {
        throw new Error('useFavorites must be used within a FavoritesProvider');
    }
    return context;
};

export const FavoritesProvider = ({ children }) => {
    const [favorites, setFavorites] = useState([]);
    const [loading, setLoading] = useState(false);
    const { user } = useAuth();

    // Fetch favorites from API when user logs in
    const fetchFavorites = useCallback(async () => {
        const token = localStorage.getItem('token');
        if (!token || !user) {
            setFavorites([]);
            return;
        }

        setLoading(true);
        try {
            const response = await fetch('http://localhost:5150/api/favorites/ids', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                setFavorites(data);
            } else {
                setFavorites([]);
            }
        } catch (error) {
            console.error('Error fetching favorites:', error);
            setFavorites([]);
        } finally {
            setLoading(false);
        }
    }, [user]);

    // Fetch favorites when user changes
    useEffect(() => {
        fetchFavorites();
    }, [fetchFavorites]);

    // Toggle favorite via API
    const toggleFavorite = async (hotelId) => {
        const token = localStorage.getItem('token');
        if (!token) {
            console.warn('User must be logged in to manage favorites');
            return false;
        }

        try {
            const response = await fetch(`http://localhost:5150/api/favorites/toggle/${hotelId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();

                if (data.isFavorite) {
                    // Add to local state
                    setFavorites(prev => [...prev, hotelId]);
                } else {
                    // Remove from local state
                    setFavorites(prev => prev.filter(id => id !== hotelId));
                }

                return data.isFavorite;
            } else {
                console.error('Failed to toggle favorite');
                return null;
            }
        } catch (error) {
            console.error('Error toggling favorite:', error);
            return null;
        }
    };

    const isFavorite = (hotelId) => {
        return favorites.includes(hotelId);
    };

    // Get full favorites list with hotel details
    const getFavoritesWithDetails = async () => {
        const token = localStorage.getItem('token');
        if (!token) {
            return [];
        }

        try {
            const response = await fetch('http://localhost:5150/api/favorites', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                return await response.json();
            }
            return [];
        } catch (error) {
            console.error('Error fetching favorites with details:', error);
            return [];
        }
    };

    const value = {
        favorites,
        loading,
        toggleFavorite,
        isFavorite,
        getFavoritesWithDetails,
        refreshFavorites: fetchFavorites
    };

    return (
        <FavoritesContext.Provider value={value}>
            {children}
        </FavoritesContext.Provider>
    );
};
