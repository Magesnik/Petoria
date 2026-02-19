import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useCurrency } from '../../context/CurrencyContext';
import { useLanguage } from '../../context/LanguageContext';

import './MyHotels.css';

const MyHotels = () => {
    const { user, isAdmin } = useAuth();
    const { convertAndFormat } = useCurrency();
    const { t } = useLanguage();
    const navigate = useNavigate();
    const [hotels, setHotels] = useState([]);
    const [unreadCounts, setUnreadCounts] = useState({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        if (!isAdmin()) {
            navigate('/');
            return;
        }
        fetchMyHotels();
    }, [isAdmin, navigate]);

    const fetchMyHotels = async () => {
        try {
            const data = await api.get('/hotels/my');
            setHotels(data);

            // Fetch unread message counts
            fetchUnreadCounts();
        } catch (err) {
            setError('Грешка при зареждане на хотелите');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const fetchUnreadCounts = async () => {
        try {
            const data = await api.get('/hotels/my/messages/unread-counts');
            const counts = {};
            data.forEach(item => {
                counts[item.hotelId] = item.unreadCount;
            });
            setUnreadCounts(counts);
        } catch (err) {
            console.error('Error fetching unread counts:', err);
        }
    };

    if (!isAdmin()) {
        return null;
    }

    return (
        <div className="my-hotels-page">


            <div className="my-hotels-container">
                <div className="page-header">
                    <h1>🏨 {t('myHotels')}</h1>
                    <Link to="/create-hotel" className="btn-create">
                        ➕ {t('addNewHotel')}
                    </Link>
                </div>

                {error && <div className="error-message">{error}</div>}

                {loading ? (
                    <div className="loading-state">
                        <div className="spinner"></div>
                        <p>Зареждане...</p>
                    </div>
                ) : hotels.length === 0 ? (
                    <div className="empty-state">
                        <h2>{t('noHotelsYet')}</h2>
                        <p>{t('createFirstHotel')}</p>
                        <Link to="/create-hotel" className="btn-create-large">
                            ➕ {t('create')} {t('hotels')}
                        </Link>
                    </div>
                ) : (
                    <div className="hotels-grid">
                        {hotels.map(hotel => (
                            <div key={hotel.id} className="hotel-card">
                                <div
                                    className="hotel-image"
                                    style={{ backgroundImage: `url(${hotel.imageUrl || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400'})` }}
                                >
                                    <div className="hotel-status">
                                        {hotel.isAvailable ? (
                                            <span className="status-available">✓ {t('active')}</span>
                                        ) : (
                                            <span className="status-unavailable">○ {t('inactive')}</span>
                                        )}
                                    </div>
                                    {unreadCounts[hotel.id] > 0 && (
                                        <Link to={`/hotel/${hotel.id}/messages`} className="notification-badge">
                                            🔔 {unreadCounts[hotel.id]}
                                        </Link>
                                    )}
                                </div>

                                <div className="hotel-content">
                                    <h3>{hotel.name}</h3>
                                    <p className="hotel-location">📍 {hotel.city}, {hotel.country}</p>
                                    <p className="hotel-price">💰 {convertAndFormat(hotel.displayPrice)}{t('perNight')}</p>

                                    <div className="hotel-stats">
                                        <span>⭐ {hotel.rating?.toFixed(1) || '0.0'}</span>
                                    </div>

                                    <div className="hotel-actions">
                                        <Link
                                            to={`/manage-hotel/${hotel.id}`}
                                            className="btn-manage"
                                        >
                                            ⚙️ {t('manage')}
                                        </Link>
                                        <Link
                                            to={`/hotel/${hotel.id}`}
                                            className="btn-view"
                                        >
                                            👁️ {t('view')}
                                        </Link>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

export default MyHotels;
