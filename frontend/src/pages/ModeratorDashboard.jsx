import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';
import './ModeratorDashboard.css'; // We'll create this CSS

const ModeratorDashboard = () => {
    const [hotels, setHotels] = useState([]);
    const [unreadCounts, setUnreadCounts] = useState({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const navigate = useNavigate();
    const { t } = useLanguage();

    useEffect(() => {
        fetchModeratedHotels();
    }, []);

    const fetchModeratedHotels = async () => {
        try {
            setLoading(true);
            const data = await api.get('/hotels/moderated');
            setHotels(data);
            fetchUnreadCounts();
            setLoading(false);
        } catch (err) {
            console.error('Error fetching moderated hotels:', err);
            setError(t('failedToLoadHotels') || 'Failed to load hotels');
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

    if (loading) return <div className="moderator-dashboard loading"><div className="spinner"></div></div>;

    return (
        <div className="moderator-dashboard">
            <div className="dashboard-header">
                <h1>🛡️ {t('moderatorDashboard') || 'Moderator Dashboard'}</h1>
                <p>{t('moderatorDashboardDesc') || 'Manage hotels where you have moderator privileges.'}</p>
            </div>

            {error && <div className="error-banner">{error}</div>}

            <div className="moderated-hotels-list">
                {hotels.length === 0 ? (
                    <div className="no-hotels">
                        <p>{t('noModeratedHotels') || 'You are not moderating any hotels yet.'}</p>
                    </div>
                ) : (
                    <div className="hotels-grid">
                        {hotels.map(hotel => (
                            <div key={hotel.id} className="moderator-hotel-card">
                                <div className="hotel-image">
                                    <img src={hotel.imageUrl || 'https://via.placeholder.com/300x200?text=No+Image'} alt={hotel.name} />
                                    {unreadCounts[hotel.id] > 0 && (
                                        <Link to={`/hotel/${hotel.id}/messages`} className="notification-badge">
                                            🔔 {unreadCounts[hotel.id]}
                                        </Link>
                                    )}
                                </div>
                                <div className="hotel-info">
                                    <h3>{hotel.name}</h3>
                                    <p className="hotel-location">📍 {hotel.city}, {hotel.country}</p>
                                    <div className="hotel-actions">
                                        <Link to={`/moderator/hotel/${hotel.id}`} className="btn btn-primary">
                                            {t('manageHotel') || 'Manage Hotel'}
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

export default ModeratorDashboard;
