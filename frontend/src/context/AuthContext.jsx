import React, { createContext, useState, useContext, useEffect } from 'react';
import { api, getAssetUrl } from '../utils/api';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const checkUser = async () => {
        try {
            const data = await api.get('/profile');
            // Ensure data is a valid object before setting it as user
            // This prevents issues where 'null', '""', or HTML strings are treated as valid users
            if (data && typeof data === 'object') {
                // Fix avatar URL if it's a relative path
                if (data.avatarUrl && data.avatarUrl.startsWith('/uploads/')) {
                    data.avatarUrl = getAssetUrl(data.avatarUrl);
                }
                setUser(data);
            } else {
                // 200 OK but null or invalid data means not logged in
                setUser(null);
            }
        } catch {
            // 401 or other error means not logged in
            setUser(null);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        checkUser();
    }, []);

    const login = (userData) => {
        // Cookie is already set by the server response in Login.jsx
        // just update state
        setUser(userData);
    };

    const logout = async () => {
        // Clear state immediately to prevent infinite loops if logout call also returns 401
        setUser(null);

        try {
            await api.post('/auth/logout');
        } catch (error) {
            console.error("Logout failed", error);
        }
        
        // Refresh to ensure clean state if needed, or just clear user
        window.location.href = '/';
    };

    useEffect(() => {
        const handleUnauthorized = () => {
            console.warn("Unauthorized request detected, logging out...");
            logout();
        };

        window.addEventListener('auth-unauthorized', handleUnauthorized);
        return () => window.removeEventListener('auth-unauthorized', handleUnauthorized);
    }, []);

    const isAdmin = () => {
        return user?.roles?.some(r => r === 'Admin' || r === 'SuperAdmin') || false;
    };

    const isSuperAdmin = () => {
        return user?.roles?.includes('SuperAdmin') || false;
    };

    return (
        <AuthContext.Provider value={{ user, login, logout, loading, isAdmin, isSuperAdmin, checkUser }}>
            {!loading && children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
