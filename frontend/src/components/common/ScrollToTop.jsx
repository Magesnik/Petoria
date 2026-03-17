import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

/** Връща скрола в началото на страницата при всяка смяна на маршрут. */
export default function ScrollToTop() {
    const { pathname } = useLocation();

    // Скролира до горната част при промяна на URL пътя
    useEffect(() => {
        window.scrollTo(0, 0);
    }, [pathname]);

    return null;
}
