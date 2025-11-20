import React, { createContext, useState, useContext } from 'react';

const LanguageContext = createContext();

export const useLanguage = () => useContext(LanguageContext);

const translations = {
    en: {
        home: 'Home',
        hotels: 'Hotels',
        destinations: 'Destinations',
        about: 'About Us',
        signIn: 'Sign In',
        register: 'Register',
        heroTitle: 'Find Your Next',
        heroSubtitle: 'Perfect Getaway',
        heroText: 'Discover luxury hotels and unique stays around the world.',
        searchPlaceholder: 'Where do you want to go?',
        searchBtn: 'Search',
        popularDestinations: 'Popular Destinations',
        from: 'From',
        night: 'night',
        loginTitle: 'Welcome Back',
        loginSubtitle: 'Enter your credentials to access your account',
        emailLabel: 'Email Address',
        passwordLabel: 'Password',
        loginBtn: 'Sign In',
        noAccount: "Don't have an account?",
        registerLink: 'Sign up',
        registerTitle: 'Create Account',
        registerSubtitle: 'Join us to book your dream vacation',
        firstNameLabel: 'First Name',
        lastNameLabel: 'Last Name',
        registerBtn: 'Create Account',
        hasAccount: 'Already have an account?',
        loginLink: 'Sign in'
    },
    bg: {
        home: 'Начало',
        hotels: 'Хотели',
        destinations: 'Дестинации',
        about: 'За Нас',
        signIn: 'Вход',
        register: 'Регистрация',
        heroTitle: 'Открий Своята',
        heroSubtitle: 'Перфектна Почивка',
        heroText: 'Открийте луксозни хотели и уникални места по света.',
        searchPlaceholder: 'Къде искате да отидете?',
        searchBtn: 'Търси',
        popularDestinations: 'Популярни Дестинации',
        from: 'От',
        night: 'нощувка',
        loginTitle: 'Добре дошли отново',
        loginSubtitle: 'Въведете данните си за достъп',
        emailLabel: 'Имейл Адрес',
        passwordLabel: 'Парола',
        loginBtn: 'Вход',
        noAccount: 'Нямате акаунт?',
        registerLink: 'Регистрирайте се',
        registerTitle: 'Създай Акаунт',
        registerSubtitle: 'Присъединете се към нас за мечтаната ваканция',
        firstNameLabel: 'Име',
        lastNameLabel: 'Фамилия',
        registerBtn: 'Създай Акаунт',
        hasAccount: 'Вече имате акаунт?',
        loginLink: 'Влезте'
    }
};

export const LanguageProvider = ({ children }) => {
    const [language, setLanguage] = useState('en');

    const t = (key) => {
        return translations[language][key] || key;
    };

    const toggleLanguage = () => {
        setLanguage((prev) => (prev === 'en' ? 'bg' : 'en'));
    };

    return (
        <LanguageContext.Provider value={{ language, toggleLanguage, t }}>
            {children}
        </LanguageContext.Provider>
    );
};
