import React, { createContext, useState, useEffect, useContext } from 'react';
import { useAuth } from './AuthContext';
import { api } from '../utils/api';

const ThemeContext = createContext();

export const useTheme = () => useContext(ThemeContext);

export const ThemeProvider = ({ children }) => {
    const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
    const { user } = useAuth();

    // Sync from user profile when user logs in
    useEffect(() => {
        if (user?.theme) {
            setTheme(user.theme);
        }
    }, [user]);

    // Apply theme to DOM
    useEffect(() => {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);

        if (theme === 'dark') {
            document.body.style.backgroundColor = '#020c1b';
            document.body.style.color = '#e6f1ff';
        } else {
            document.body.style.backgroundColor = '#ffffff';
            document.body.style.color = '#112240';
        }
    }, [theme]);

    const toggleTheme = () => {
        const newTheme = theme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);

        // Sync to backend if logged in
        if (user) {
            api.put('/profile', { theme: newTheme })
                .catch(err => console.error("Failed to save theme preference", err));
        }
    };

    return (
        <ThemeContext.Provider value={{ theme, toggleTheme }}>
            {children}
        </ThemeContext.Provider>
    );
};
