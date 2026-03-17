import React, { createContext, useState, useContext, useEffect } from 'react';
import { useAuth } from './AuthContext';
import { api } from '../utils/api';

/** Контекст за валута: BGN/EUR/USD конверсия, форматиране, синхронизация с профила */
const CurrencyContext = createContext();

/** Хук за достъп до контекста за валута */
export const useCurrency = () => useContext(CurrencyContext);

// Обменни курсове спрямо BGN (базова валута)
const EXCHANGE_RATES = {
    BGN: 1,
    EUR: 0.5113, // 1 BGN = 0.5113 EUR
    USD: 0.5556  // 1 BGN = 0.5556 USD
};

// Символи на валутите
const CURRENCY_SYMBOLS = {
    BGN: 'лв',
    EUR: '€',
    USD: '$'
};

/** Доставчик на контекста за валута */
export const CurrencyProvider = ({ children }) => {
    const [currency, setCurrency] = useState(() => {
        const saved = localStorage.getItem('currency');
        return saved || 'BGN';
    });
    const { user } = useAuth();

    // Синхронизация от потребителския профил при логване
    useEffect(() => {
        if (user?.currency && EXCHANGE_RATES[user.currency]) {
            setCurrency(user.currency);
            localStorage.setItem('currency', user.currency);
        }
    }, [user]);

    useEffect(() => {
        localStorage.setItem('currency', currency);
    }, [currency]);

    /** Конвертира цена от BGN към избраната валута */
    const convertPrice = (priceInBGN) => {
        if (!priceInBGN || isNaN(priceInBGN)) return 0;
        return priceInBGN * EXCHANGE_RATES[currency];
    };

    /** Конвертира цена обратно към BGN от избраната валута */
    const convertToBase = (priceInCurrentCurrency) => {
        if (!priceInCurrentCurrency || isNaN(priceInCurrentCurrency)) return 0;
        return priceInCurrentCurrency / EXCHANGE_RATES[currency];
    };

    /** Форматира цена със символ на валутата */
    const formatPrice = (price, showCurrency = true) => {
        if (!price || isNaN(price)) return '0';

        const formatted = price.toFixed(2);

        if (!showCurrency) return formatted;

        // За BGN символът е след числото
        if (currency === 'BGN') {
            return `${formatted} ${CURRENCY_SYMBOLS[currency]}`;
        }

        // За EUR и USD символът е преди числото
        return `${CURRENCY_SYMBOLS[currency]}${formatted}`;
    };

    /** Конвертира и форматира цена наведнъж */
    const convertAndFormat = (priceInBGN, showCurrency = true) => {
        const converted = convertPrice(priceInBGN);
        return formatPrice(converted, showCurrency);
    };

    /** Сменя валутата и синхронизира с бекенда ако е логнат */
    const changeCurrency = (newCurrency) => {
        if (EXCHANGE_RATES[newCurrency]) {
            setCurrency(newCurrency);
            localStorage.setItem('currency', newCurrency);

            // Синхронизация с бекенда ако е логнат
            if (user) {
                api.put('/profile', { currency: newCurrency })
                    .catch(err => console.error("Грешка при запис на предпочитание за валута", err));
            }
        }
    };

    return (
        <CurrencyContext.Provider
            value={{
                currency,
                changeCurrency,
                convertPrice,
                convertToBase,
                formatPrice,
                convertAndFormat,
                availableCurrencies: Object.keys(EXCHANGE_RATES),
                currencySymbol: CURRENCY_SYMBOLS[currency]
            }}
        >
            {children}
        </CurrencyContext.Provider>
    );
};
