import React, { createContext, useState, useContext, useEffect } from 'react';
import { api, getAssetUrl } from '../utils/api';

/** Контекст за автентикация: потребителски данни, login/logout, проверка на роли */
const AuthContext = createContext(null);

/** Доставчик на автентикационния контекст */
export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    /** Проверява дали потребителят е логнат чрез GET /profile */
    const checkUser = async () => {
        try {
            const data = await api.get('/profile');
            // Проверка дали данните са валиден обект преди да се запишат
            if (data && typeof data === 'object') {
                // Поправка на URL на аватара ако е относителен път
                if (data.avatarUrl && data.avatarUrl.startsWith('/uploads/')) {
                    data.avatarUrl = getAssetUrl(data.avatarUrl);
                }
                setUser(data);
            } else {
                // 200 OK но невалидни данни означава, че не е логнат
                setUser(null);
            }
        } catch {
            // 401 или друга грешка означава, че не е логнат
            setUser(null);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        checkUser();
    }, []);

    /** Записва потребителските данни в state при успешен вход */
    const login = (userData) => {
        // Бисквитката вече е зададена от сървъра в Login.jsx
        setUser(userData);
    };

    /** Излизане от профила и пренасочване към началната страница */
    const logout = async () => {
        // Изчистваме state веднага за да предотвратим безкрайни цикли при 401
        setUser(null);

        try {
            await api.post('/auth/logout');
        } catch (error) {
            console.error("Logout failed", error);
        }

        // Презареждане за чисто състояние
        window.location.href = '/';
    };

    // Слушател за неоторизирани заявки — автоматичен logout при 401
    useEffect(() => {
        const handleUnauthorized = () => {
            console.warn("Unauthorized request detected, logging out...");
            logout();
        };

        window.addEventListener('auth-unauthorized', handleUnauthorized);
        return () => window.removeEventListener('auth-unauthorized', handleUnauthorized);
    }, []);

    /** Проверява дали потребителят е администратор */
    const isAdmin = () => {
        return user?.roles?.some(r => r === 'Admin' || r === 'SuperAdmin') || false;
    };

    /** Проверява дали потребителят е супер администратор */
    const isSuperAdmin = () => {
        return user?.roles?.includes('SuperAdmin') || false;
    };

    return (
        <AuthContext.Provider value={{ user, login, logout, loading, isAdmin, isSuperAdmin, checkUser }}>
            {!loading && children}
        </AuthContext.Provider>
    );
};

/** Хук за достъп до автентикационния контекст */
export const useAuth = () => useContext(AuthContext);
