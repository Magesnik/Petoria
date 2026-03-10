import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { api } from '../utils/api';
import { useAuth } from './AuthContext';

const CartContext = createContext(null);

export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) throw new Error('useCart must be used within a CartProvider');
    return context;
};

export const CartProvider = ({ children }) => {
    const { user } = useAuth();
    const [cartItems, setCartItems] = useState([]);
    const [cartLoading, setCartLoading] = useState(false);

    // Fetch cart from API when user logs in
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
            console.error('Failed to load cart:', err);
            setCartItems([]);
        } finally {
            setCartLoading(false);
        }
    }, [user]);

    useEffect(() => {
        fetchCart();
    }, [fetchCart]);

    // Add item to cart via API
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
            console.error('Failed to add to cart:', err);
            throw err;
        }
    };

    // Remove item from cart via API
    const removeFromCart = async (cartId) => {
        try {
            await api.delete(`/cart/${cartId}`);
            setCartItems(prev => prev.filter(item => item.id !== cartId));
        } catch (err) {
            console.error('Failed to remove from cart:', err);
            throw err;
        }
    };

    // Clear entire cart via API
    const clearCart = async () => {
        try {
            await api.delete('/cart');
            setCartItems([]);
        } catch (err) {
            console.error('Failed to clear cart:', err);
            throw err;
        }
    };

    const cartCount = cartItems.length;

    return (
        <CartContext.Provider value={{ cartItems, cartCount, cartLoading, addToCart, removeFromCart, clearCart, fetchCart }}>
            {children}
        </CartContext.Provider>
    );
};

export default CartContext;
