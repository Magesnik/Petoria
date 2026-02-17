import React, { createContext, useState, useContext, useEffect } from 'react';
import { api } from '../utils/api';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const checkUser = async () => {
        try {
            const data = await api.get('/profile');
            if (data) {
                // Fix avatar URL if it's a relative path
                if (data.avatarUrl && data.avatarUrl.startsWith('/uploads/')) {
                    data.avatarUrl = `http://localhost:5150${data.avatarUrl}`;
                }
                setUser(data);
            } else {
                // 200 OK but null data means not logged in
                setUser(null);
            }
        } catch (error) {
            // 401 or other error means not logged in
            // console.debug("Not authenticated or session expired");
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
        try {
            await api.post('/auth/logout');
        } catch (error) {
            console.error("Logout failed", error);
        }
        setUser(null);
        // Refresh to ensure clean state if needed, or just clear user
        window.location.href = '/';
    };

    const isAdmin = () => {
        // Roles might be returned from profile endpoint? 
        // ProfileDto in backend Controller returns: Id, Email, FirstName, LastName, AvatarUrl. 
        // It DOES NOT currently return Roles. We need to check this.
        // Let's assume we need to update ProfileController to return roles or store them in state differently.
        // For now, let's look at what Login returns. Login returns AuthResponse which has Roles.
        // But /api/profile might need to return Roles too for persistence on refresh.
        return user?.roles?.includes('Admin') || false;
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
