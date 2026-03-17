import React, { createContext, useState, useEffect, useContext } from 'react';
import { useAuth } from './AuthContext';
import { api } from '../utils/api';

/** Контекст за тема: светла/тъмна, toggle, data-theme атрибут */
const ThemeContext = createContext();

/** Хук за достъп до контекста за тема */
export const useTheme = () => useContext(ThemeContext);

/** Доставчик на контекста за тема */
export const ThemeProvider = ({ children }) => {
    const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
    const { user } = useAuth();

    // Синхронизация от потребителския профил при логване
    useEffect(() => {
        if (user?.theme) {
            setTheme(user.theme);
        }
    }, [user]);

    // Прилагане на темата към DOM елемента
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

    /** Превключва между светла и тъмна тема */
    const toggleTheme = () => {
        const newTheme = theme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);

        // Синхронизация с бекенда ако е логнат
        if (user) {
            api.put('/profile', { theme: newTheme })
                .catch(err => console.error("Грешка при запис на предпочитание за тема", err));
        }
    };

    return (
        <ThemeContext.Provider value={{ theme, toggleTheme }}>
            {children}
        </ThemeContext.Provider>
    );
};
