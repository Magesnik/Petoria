import React, { createContext, useState, useContext } from 'react';
import en from '../locales/en.json';
import bg from '../locales/bg.json';

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

    const t = (key) => {
        return translations[language][key] || key;
    };

    const toggleLanguage = () => {
        setLanguage((prev) => {
            const newLang = prev === 'en' ? 'bg' : 'en';
            // Save to localStorage
            localStorage.setItem('language', newLang);
            return newLang;
        });
    };

    return (
        <LanguageContext.Provider value={{ language, toggleLanguage, t }}>
            {children}
        </LanguageContext.Provider>
    );
};
