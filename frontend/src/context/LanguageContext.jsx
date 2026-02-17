import React, { createContext, useState, useEffect, useContext } from 'react';
import en from '../locales/en.json';
import bg from '../locales/bg.json';
import { useAuth } from './AuthContext';
import { api } from '../utils/api';

const LanguageContext = createContext();

export const useLanguage = () => useContext(LanguageContext);

const translations = {
    en,
    bg
};

export const LanguageProvider = ({ children }) => {
    // Initialize language from localStorage or default to 'en'
    const [language, setLanguage] = useState(() => {
        const savedLanguage = localStorage.getItem('language');
        return savedLanguage || 'en';
    });
    const { user } = useAuth();

    // Sync from user profile when user logs in
    useEffect(() => {
        if (user?.language) {
            setLanguage(user.language);
            localStorage.setItem('language', user.language);
        }
    }, [user]);

    const t = (key) => {
        return translations[language][key] || key;
    };

    const toggleLanguage = () => {
        const newLang = language === 'en' ? 'bg' : 'en';
        setLanguage(newLang);
        localStorage.setItem('language', newLang);

        // Sync to backend if logged in
        if (user) {
            api.put('/profile', { language: newLang })
                .catch(err => console.error("Failed to save language preference", err));
        }
    };

    return (
        <LanguageContext.Provider value={{ language, toggleLanguage, t }}>
            {children}
        </LanguageContext.Provider>
    );
};
