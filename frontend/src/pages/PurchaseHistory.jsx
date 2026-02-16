import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { Link, useNavigate } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import { useCurrency } from '../context/CurrencyContext';
import Header from '../components/Header';
import './PurchaseHistory.css';

const PurchaseHistory = () => {
    const { t, language } = useLanguage();
    const { user } = useAuth();
    const { convertAndFormat } = useCurrency();
    const navigate = useNavigate();
    const [reservations, setReservations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        if (!user) {
            navigate('/login');
            return;
        }
        fetchReservations();
    }, [user, navigate]);

    const fetchReservations = async () => {
        try {
            const data = await api.get('/reservations/my');
            setReservations(data);
        } catch (err) {
            console.error('Error fetching reservations:', err);
            setError(t('errorFetchingReservations'));
        } finally {
            setLoading(false);
        }
    };

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

    if (loading) {
        return (
            <>
                <Header />
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
            <Header />
            <div className="purchase-history-page">
                <div className="purchase-hero">
                    <h1>📋 {t('myReservations')}</h1>
                    <p>{t('reviewHistory')}</p>
                </div>

                <div className="purchase-container">
                    {error && <div className="error-message">{error}</div>}

                    {reservations.length === 0 ? (
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
                            {reservations.map((reservation) => (
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
                                            {reservation.status?.toLowerCase() === 'confirmed' && (
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
