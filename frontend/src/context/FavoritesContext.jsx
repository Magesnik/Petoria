import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { api } from '../utils/api';
import { useAuth } from './AuthContext';

/** Контекст за любими хотели: списък с ID-та, toggle, проверка, детайли */
const FavoritesContext = createContext();

/** Хук за достъп до контекста за любими */
export const useFavorites = () => {
    const context = useContext(FavoritesContext);
    if (!context) {
        throw new Error('useFavorites must be used within a FavoritesProvider');
    }
    return context;
};

/** Доставчик на контекста за любими хотели */
export const FavoritesProvider = ({ children }) => {
    const [favorites, setFavorites] = useState([]);
    const [loading, setLoading] = useState(false);
    const { user } = useAuth();

    /** Зарежда списъка с любими ID-та от API */
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
            console.error('Грешка при зареждане на любими:', error);
            setFavorites([]);
        } finally {
            setLoading(false);
        }
    }, [user]);

    // Зареждане на любими при смяна на потребител
    useEffect(() => {
        fetchFavorites();
    }, [fetchFavorites]);

    /** Превключва любим статус на хотел чрез API */
    const toggleFavorite = async (hotelId) => {
        if (!user) {
            console.warn('Потребителят трябва да е логнат за да управлява любими');
            return false;
        }

        try {
            const data = await api.post(`/favorites/toggle/${hotelId}`);

            if (data.isFavorite) {
                // Добавяне към локалния state
                setFavorites(prev => [...prev, hotelId]);
            } else {
                // Премахване от локалния state
                setFavorites(prev => prev.filter(id => id !== hotelId));
            }

            return data.isFavorite;
        } catch (error) {
            console.error('Грешка при превключване на любим:', error);
            return null;
        }
    };

    /** Проверява дали хотел е в любими */
    const isFavorite = (hotelId) => {
        return favorites.includes(hotelId);
    };

    /** Връща пълен списък с любими хотели с детайли */
    const getFavoritesWithDetails = async () => {
        if (!user) {
            return [];
        }

        try {
            const data = await api.get('/favorites');
            return data;
        } catch (error) {
            console.error('Грешка при зареждане на любими с детайли:', error);
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
