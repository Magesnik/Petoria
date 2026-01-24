import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import Header from '../components/Header';
import './PurchaseHistory.css';

const PurchaseHistory = () => {
    const { t } = useLanguage();
    const { user } = useAuth();
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
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5150/api/reservations/my', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Failed to fetch reservations');

            const data = await response.json();
            setReservations(data);
        } catch (err) {
            console.error('Error fetching reservations:', err);
            setError('Грешка при зареждане на резервациите');
        } finally {
            setLoading(false);
        }
    };

    const handleCancelReservation = async (reservationId) => {
        if (!window.confirm('Сигурни ли сте, че искате да отмените тази резервация?')) {
            return;
        }

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5150/api/reservations/${reservationId}/cancel`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Failed to cancel reservation');

            // Refresh list
            fetchReservations();
        } catch (err) {
            console.error('Error cancelling reservation:', err);
            setError('Грешка при отмяна на резервацията');
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
                return 'Потвърдена';
            case 'pending':
                return 'Изчакваща';
            case 'cancelled':
                return 'Отменена';
            case 'completed':
                return 'Завършена';
            default:
                return status;
        }
    };

    const formatDate = (dateStr) => {
        return new Date(dateStr).toLocaleDateString('bg-BG', {
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
                    <h1>📋 Моите резервации</h1>
                    <p>Преглед на вашите минали и предстоящи резервации</p>
                </div>

                <div className="purchase-container">
                    {error && <div className="error-message">{error}</div>}

                    {reservations.length === 0 ? (
                        <div className="empty-state">
                            <div className="empty-icon">🏨</div>
                            <h2>Нямате резервации</h2>
                            <p>Все още нямате направени резервации. Разгледайте нашите хотели!</p>
                            <Link to="/hotels" className="btn btn-primary">
                                Разгледай хотели
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
                                                <span className="date-label">Настаняване</span>
                                                <span className="date-value">{formatDate(reservation.checkInDate)}</span>
                                            </div>
                                            <div className="date-separator">→</div>
                                            <div className="date-item">
                                                <span className="date-label">Напускане</span>
                                                <span className="date-value">{formatDate(reservation.checkOutDate)}</span>
                                            </div>
                                        </div>

                                        <div className="reservation-info">
                                            <span>🌙 {reservation.numberOfNights} нощувки</span>
                                            <span>🚪 {reservation.numberOfRooms} {reservation.numberOfRooms === 1 ? 'стая' : 'стаи'}</span>
                                        </div>
                                    </div>

                                    <div className="reservation-summary">
                                        <div className="price-breakdown">
                                            <span className="price-detail">
                                                {reservation.pricePerNight} лв × {reservation.numberOfNights} нощи
                                                {reservation.numberOfRooms > 1 && ` × ${reservation.numberOfRooms} стаи`}
                                            </span>
                                        </div>
                                        <div className="total-price">
                                            <span className="price-label">Обща сума</span>
                                            <span className="price-value">{reservation.totalPrice} лв</span>
                                        </div>

                                        <div className="reservation-actions">
                                            <Link to={`/hotel/${reservation.hotelId}`} className="btn btn-view">
                                                Виж хотел
                                            </Link>
                                            {reservation.status?.toLowerCase() === 'confirmed' && (
                                                <button
                                                    className="btn btn-cancel"
                                                    onClick={() => handleCancelReservation(reservation.id)}
                                                >
                                                    Отмени
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
