import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { api } from '../utils/api';
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
        if (!user) {
            setFavorites([]);
            return;
        }

        setLoading(true);
        try {
            const data = await api.get('/favorites/ids');
            setFavorites(data);
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
        if (!user) {
            console.warn('User must be logged in to manage favorites');
            return false;
        }

        try {
            const data = await api.post(`/favorites/toggle/${hotelId}`);

            if (data.isFavorite) {
                // Add to local state
                setFavorites(prev => [...prev, hotelId]);
            } else {
                // Remove from local state
                setFavorites(prev => prev.filter(id => id !== hotelId));
            }

            return data.isFavorite;
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
        if (!user) {
            return [];
        }

        try {
            const data = await api.get('/favorites');
            return data;
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
