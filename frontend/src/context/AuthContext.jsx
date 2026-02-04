import React, { createContext, useState, useContext, useEffect } from 'react';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Check for stored user data on mount
        const storedUser = localStorage.getItem('user');
        const storedToken = localStorage.getItem('token');

        if (storedUser && storedToken) {
            try {
                const parsedUser = JSON.parse(storedUser);

                // Fix avatar URL if it's a relative path
                if (parsedUser.avatarUrl && parsedUser.avatarUrl.startsWith('/uploads/')) {
                    parsedUser.avatarUrl = `http://localhost:5150${parsedUser.avatarUrl}`;
                }

                setUser(parsedUser);
            } catch (error) {
                console.error("Failed to parse user data:", error);
                localStorage.removeItem('user');
                localStorage.removeItem('token');
            }
        }
        setLoading(false);
    }, []);

    const login = (userData, token) => {
        setUser(userData);
        localStorage.setItem('user', JSON.stringify(userData));
        localStorage.setItem('token', token);
    };

    const logout = () => {
        setUser(null);
        localStorage.removeItem('user');
        localStorage.removeItem('token');
    };

    const isAdmin = () => {
        return user?.roles?.includes('Admin') || false;
    };

    const isSuperAdmin = () => {
        return user?.roles?.includes('SuperAdmin') || false;
    };

    return (
        <AuthContext.Provider value={{ user, login, logout, loading, isAdmin, isSuperAdmin }}>
            {!loading && children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
