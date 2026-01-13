import React, { createContext, useContext, useState, useEffect } from 'react';

const FavoritesContext = createContext();

export const useFavorites = () => {
    const context = useContext(FavoritesContext);
    if (!context) {
        throw new Error('useFavorites must be used within a FavoritesProvider');
    }
    return context;
};

export const FavoritesProvider = ({ children }) => {
    const [favorites, setFavorites] = useState(() => {
        // Initialize from localStorage
        const stored = localStorage.getItem('hotelFavorites');
        return stored ? JSON.parse(stored) : [];
    });

    // Persist to localStorage whenever favorites change
    useEffect(() => {
        localStorage.setItem('hotelFavorites', JSON.stringify(favorites));
    }, [favorites]);

    const toggleFavorite = (hotelId) => {
        setFavorites((prev) => {
            if (prev.includes(hotelId)) {
                // Remove from favorites
                return prev.filter(id => id !== hotelId);
            } else {
                // Add to favorites
                return [...prev, hotelId];
            }
        });
    };

    const isFavorite = (hotelId) => {
        return favorites.includes(hotelId);
    };

    const value = {
        favorites,
        toggleFavorite,
        isFavorite
    };

    return (
        <FavoritesContext.Provider value={value}>
            {children}
        </FavoritesContext.Provider>
    );
};
