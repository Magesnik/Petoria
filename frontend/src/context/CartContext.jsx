import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { api } from '../utils/api';
import { useAuth } from './AuthContext';

/** Контекст за количка: зареждане, добавяне, премахване, изчистване, брояч */
const CartContext = createContext(null);

/** Хук за достъп до контекста на количката */
export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) throw new Error('useCart must be used within a CartProvider');
    return context;
};

/** Доставчик на контекста за количката */
export const CartProvider = ({ children }) => {
    const { user } = useAuth();
    const [cartItems, setCartItems] = useState([]);
    const [cartLoading, setCartLoading] = useState(false);

    /** Зарежда количката от API при логване на потребител */
    const fetchCart = useCallback(async () => {
        if (!user) {
            setCartItems([]);
            return;
        }
        try {
            setCartLoading(true);
            const data = await api.get('/cart');
            setCartItems(data);
        } catch (err) {
            console.error('Грешка при зареждане на количката:', err);
            setCartItems([]);
        } finally {
            setCartLoading(false);
        }
    }, [user]);

    useEffect(() => {
        fetchCart();
    }, [fetchCart]);

    /** Добавя артикул в количката чрез API */
    const addToCart = async (item) => {
        if (!user) return null;
        try {
            const payload = {
                hotelId: item.hotelId,
                roomTypeId: item.roomTypeId,
                checkInDate: item.checkInDate,
                checkOutDate: item.checkOutDate,
                numberOfRooms: item.numberOfRooms || 1,
            };
            const newItem = await api.post('/cart', payload);
            setCartItems(prev => [newItem, ...prev]);
            return newItem;
        } catch (err) {
            console.error('Грешка при добавяне в количката:', err);
            throw err;
        }
    };

    /** Премахва артикул от количката чрез API */
    const removeFromCart = async (cartId) => {
        try {
            await api.delete(`/cart/${cartId}`);
            setCartItems(prev => prev.filter(item => item.id !== cartId));
        } catch (err) {
            console.error('Грешка при премахване от количката:', err);
            throw err;
        }
    };

    /** Изчиства цялата количка чрез API */
    const clearCart = async () => {
        try {
            await api.delete('/cart');
            setCartItems([]);
        } catch (err) {
            console.error('Грешка при изчистване на количката:', err);
            throw err;
        }
    };

    // Брой артикули в количката
    const cartCount = cartItems.length;

    return (
        <CartContext.Provider value={{ cartItems, cartCount, cartLoading, addToCart, removeFromCart, clearCart, fetchCart }}>
            {children}
        </CartContext.Provider>
    );
};

export default CartContext;
