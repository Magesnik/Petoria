import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { Link, useNavigate } from 'react-router-dom';
import { useLanguage } from '../../context/LanguageContext';
import { useAuth } from '../../context/AuthContext';
import { useCurrency } from '../../context/CurrencyContext';

import './PurchaseHistory.css';

/** Страница с история на резервациите — активни, минали и отменени. */
const PurchaseHistory = () => {
    const { t, language } = useLanguage();
    const { user } = useAuth();
    const { convertAndFormat } = useCurrency();
    const navigate = useNavigate();
    const [reservations, setReservations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // Активен таб: 'active' | 'past' | 'cancelled'
    const [activeTab, setActiveTab] = useState('active');

    const fetchReservations = React.useCallback(async () => {
        try {
            const data = await api.get('/reservations/my');
            setReservations(data);
        } catch (err) {
            console.error('Error fetching reservations:', err);
            setError(t('errorFetchingReservations'));
        } finally {
            setLoading(false);
        }
    }, [t]);

    useEffect(() => {
        if (!user) {
            navigate('/login');
            return;
        }
        fetchReservations();
    }, [user, navigate, fetchReservations]);

    const handleCancelReservation = async (reservationId) => {
        if (!window.confirm(t('confirmCancelReservation'))) {
            return;
        }

        try {
            await api.put(`/reservations/${reservationId}/cancel`);
            // Refresh list
            fetchReservations();
        } catch (err) {
            console.error('Error cancelling reservation:', err);
            setError(t('errorCancellingReservation'));
        }
    };

    const getStatusClass = (status) => {
        switch (status?.toLowerCase()) {
            case 'confirmed':
                return 'status-confirmed';
            case 'pending':
                return 'status-pending';
            case 'cancelled':
                return 'status-cancelled';
            case 'completed':
                return 'status-completed';
            default:
                return '';
        }
    };

    const getStatusLabel = (status) => {
        switch (status?.toLowerCase()) {
            case 'confirmed':
                return t('statusConfirmed');
            case 'pending':
                return t('statusPending');
            case 'cancelled':
                return t('statusCancelled');
            case 'completed':
                return t('statusCompleted');
            default:
                return status;
        }
    };

    const formatDate = (dateStr) => {
        return new Date(dateStr).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    };

    const now = new Date();
    const activeReservations = reservations.filter(r =>
        (r.status === 'Confirmed' || r.status === 'Pending') && new Date(r.checkOutDate) >= now
    );
    const pastReservations = reservations.filter(r =>
        r.status === 'Completed' || (r.status === 'Confirmed' && new Date(r.checkOutDate) < now)
    );
    const cancelledReservations = reservations.filter(r =>
        r.status === 'Cancelled'
    );

    const getDisplayedReservations = () => {
        if (activeTab === 'active') return activeReservations;
        if (activeTab === 'past') return pastReservations;
        if (activeTab === 'cancelled') return cancelledReservations;
        return [];
    };

    const displayedReservations = getDisplayedReservations();

    if (loading) {
        return (
            <>
                <div className="purchase-history-page">
                    <div className="loading-state">
                        <div className="spinner"></div>
                        <p>Зареждане...</p>
                    </div>
                </div>
            </>
        );
    }

    return (
        <>
            <div className="purchase-history-page">
                <div className="purchase-hero">
                    <h1>📋 {t('myReservations')}</h1>
                    <p>{t('reviewHistory')}</p>
                </div>

                <div className="purchase-container">
                    <div className="purchase-tabs">
                        <button
                            className={`purchase-tab ${activeTab === 'active' ? 'active' : ''}`}
                            onClick={() => setActiveTab('active')}
                        >
                            {t('activeReservations') || 'Предстоящи'} ({activeReservations.length})
                        </button>
                        <button
                            className={`purchase-tab ${activeTab === 'past' ? 'active' : ''}`}
                            onClick={() => setActiveTab('past')}
                        >
                            {t('pastReservations') || 'Минали'} ({pastReservations.length})
                        </button>
                        <button
                            className={`purchase-tab ${activeTab === 'cancelled' ? 'active' : ''}`}
                            onClick={() => setActiveTab('cancelled')}
                        >
                            {t('cancelledReservations') || 'Отменени'} ({cancelledReservations.length})
                        </button>
                    </div>

                    {error && <div className="error-message">{error}</div>}

                    {displayedReservations.length === 0 ? (
                        <div className="empty-state">
                            <div className="empty-icon">🏨</div>
                            <h2>{t('noReservations')}</h2>
                            <p>{t('noReservationsText')}</p>
                            <Link to="/hotels" className="btn btn-primary">
                                {t('browseHotels')}
                            </Link>
                        </div>
                    ) : (
                        <div className="reservations-list">
                            {displayedReservations.map((reservation) => (
                                <div key={reservation.id} className={`reservation-card ${getStatusClass(reservation.status)}`}>
                                    <div className="reservation-image">
                                        <img
                                            src={reservation.hotelImageUrl || '/placeholder-hotel.jpg'}
                                            alt={reservation.hotelName}
                                        />
                                        <span className={`status-badge ${getStatusClass(reservation.status)}`}>
                                            {getStatusLabel(reservation.status)}
                                        </span>
                                    </div>

                                    <div className="reservation-details">
                                        <h3>{reservation.hotelName}</h3>
                                        <p className="room-type">🛏️ {reservation.roomTypeName}</p>

                                        <div className="reservation-dates">
                                            <div className="date-item">
                                                <span className="date-label">{t('checkIn')}</span>
                                                <span className="date-value">{formatDate(reservation.checkInDate)}</span>
                                            </div>
                                            <div className="date-separator">→</div>
                                            <div className="date-item">
                                                <span className="date-label">{t('checkOut')}</span>
                                                <span className="date-value">{formatDate(reservation.checkOutDate)}</span>
                                            </div>
                                        </div>

                                        <div className="reservation-info">
                                            <span>🌙 {reservation.numberOfNights} {t('nights')}</span>
                                            <span>🚪 {reservation.numberOfRooms} {reservation.numberOfRooms === 1 ? t('room') : t('rooms')}</span>
                                        </div>
                                    </div>

                                    <div className="reservation-summary">
                                        <div className="price-breakdown">
                                            <span className="price-detail">
                                                {convertAndFormat(reservation.pricePerNight)} × {reservation.numberOfNights} {t('nights')}
                                                {reservation.numberOfRooms > 1 && ` × ${reservation.numberOfRooms} ${reservation.numberOfRooms === 1 ? t('room') : t('rooms')}`}
                                            </span>
                                        </div>
                                        <div className="total-price">
                                            <span className="price-label">{t('totalAmount')}</span>
                                            <span className="price-value">{convertAndFormat(reservation.totalPrice)}</span>
                                        </div>

                                        <div className="reservation-actions">
                                            <Link to={`/hotel/${reservation.hotelId}`} className="btn btn-view">
                                                {t('viewHotel')}
                                            </Link>
                                            {reservation.status?.toLowerCase() === 'confirmed' && new Date(reservation.checkInDate).setHours(23, 59, 59, 999) >= now.getTime() && (
                                                <button
                                                    className="btn btn-cancel"
                                                    onClick={() => handleCancelReservation(reservation.id)}
                                                >
                                                    {t('cancelReservation')}
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};

export default PurchaseHistory;
