import React, { createContext, useState, useContext, useEffect } from 'react';
import { useAuth } from './AuthContext';
import { api } from '../utils/api';

const CurrencyContext = createContext();

export const useCurrency = () => useContext(CurrencyContext);

// Exchange rates relative to BGN (base currency)
const EXCHANGE_RATES = {
    BGN: 1,
    EUR: 0.5113, // 1 BGN = 0.5113 EUR
    USD: 0.5556  // 1 BGN = 0.5556 USD
};

const CURRENCY_SYMBOLS = {
    BGN: 'лв',
    EUR: '€',
    USD: '$'
};

export const CurrencyProvider = ({ children }) => {
    const [currency, setCurrency] = useState(() => {
        const saved = localStorage.getItem('currency');
        return saved || 'BGN';
    });
    const { user } = useAuth();

    // Sync from user profile when user logs in
    useEffect(() => {
        if (user?.currency && EXCHANGE_RATES[user.currency]) {
            setCurrency(user.currency);
            localStorage.setItem('currency', user.currency);
        }
    }, [user]);

    useEffect(() => {
        localStorage.setItem('currency', currency);
    }, [currency]);

    // Convert price from BGN to selected currency
    const convertPrice = (priceInBGN) => {
        if (!priceInBGN || isNaN(priceInBGN)) return 0;
        return priceInBGN * EXCHANGE_RATES[currency];
    };

    // Format price with currency symbol
    const formatPrice = (price, showCurrency = true) => {
        if (!price || isNaN(price)) return '0';

        const formatted = price.toFixed(2);

        if (!showCurrency) return formatted;

        // For BGN, symbol goes after the number
        if (currency === 'BGN') {
            return `${formatted} ${CURRENCY_SYMBOLS[currency]}`;
        }

        // For EUR and USD, symbol goes before the number
        return `${CURRENCY_SYMBOLS[currency]}${formatted}`;
    };

    // Convenience function to convert and format in one go
    const convertAndFormat = (priceInBGN, showCurrency = true) => {
        const converted = convertPrice(priceInBGN);
        return formatPrice(converted, showCurrency);
    };

    const changeCurrency = (newCurrency) => {
        if (EXCHANGE_RATES[newCurrency]) {
            setCurrency(newCurrency);
            localStorage.setItem('currency', newCurrency);

            // Sync to backend if logged in
            if (user) {
                api.put('/profile', { currency: newCurrency })
                    .catch(err => console.error("Failed to save currency preference", err));
            }
        }
    };

    return (
        <CurrencyContext.Provider
            value={{
                currency,
                changeCurrency,
                convertPrice,
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
