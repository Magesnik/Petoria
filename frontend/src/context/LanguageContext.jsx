import React, { createContext, useState, useEffect, useContext } from 'react';
import en from '../locales/en.json';
import bg from '../locales/bg.json';
import { useAuth } from './AuthContext';
import { api } from '../utils/api';

/** Контекст за език: BG/EN, функция t() за превод, синхронизация с профила */
const LanguageContext = createContext();

/** Хук за достъп до езиковия контекст */
export const useLanguage = () => useContext(LanguageContext);

// Речници за превод
const translations = {
    en,
    bg
};

/** Доставчик на езиковия контекст */
export const LanguageProvider = ({ children }) => {
    // Инициализация на език от localStorage или по подразбиране 'en'
    const [language, setLanguage] = useState(() => {
        const savedLanguage = localStorage.getItem('language');
        return savedLanguage || 'en';
    });
    const { user } = useAuth();

    // Синхронизация от потребителския профил при логване
    useEffect(() => {
        if (user?.language) {
            setLanguage(user.language);
            localStorage.setItem('language', user.language);
        }
    }, [user]);

    /** Връща превод по ключ за текущия език */
    const t = (key) => {
        return translations[language][key] || key;
    };

    /** Превключва между BG и EN и синхронизира с бекенда */
    const toggleLanguage = () => {
        const newLang = language === 'en' ? 'bg' : 'en';
        setLanguage(newLang);
        localStorage.setItem('language', newLang);

        // Синхронизация с бекенда ако е логнат
        if (user) {
            api.put('/profile', { language: newLang })
                .catch(err => console.error("Грешка при запис на езиково предпочитание", err));
        }
    };

    return (
        <LanguageContext.Provider value={{ language, toggleLanguage, t }}>
            {children}
        </LanguageContext.Provider>
    );
};
