import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import { useCurrency } from '../context/CurrencyContext';
import DateRangeCalendar from './DateRangeCalendar';
import './BookingWidget.css';

const BookingWidget = ({ hotelId, onBookingComplete }) => {
    const { user } = useAuth();
    const { t } = useLanguage();
    const { convertAndFormat } = useCurrency();

    // State for room types
    const [roomTypes, setRoomTypes] = useState([]);
    const [selectedRoomType, setSelectedRoomType] = useState(null);

    // State for dates
    const [checkInDate, setCheckInDate] = useState('');
    const [checkOutDate, setCheckOutDate] = useState('');
    const [numberOfRooms, setNumberOfRooms] = useState(1);

    // State for availability
    const [availability, setAvailability] = useState([]);
    const [blockedDates, setBlockedDates] = useState(new Set());

    // State for price calculation
    const [priceInfo, setPriceInfo] = useState(null);

    // State for loading and errors
    const [loading, setLoading] = useState(true);
    const [bookingLoading, setBookingLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    // Get today's date for min date attribute
    const today = new Date().toISOString().split('T')[0];

    // Fetch room types when component mounts
    useEffect(() => {
        fetchRoomTypes();
    }, [hotelId]);

    // Fetch availability when room type changes
    useEffect(() => {
        if (selectedRoomType) {
            fetchAvailability();
        }
    }, [selectedRoomType]);

    // Calculate price when dates or room selection changes
    useEffect(() => {
        if (selectedRoomType && checkInDate && checkOutDate) {
            calculatePrice();
        } else {
            setPriceInfo(null);
        }
    }, [selectedRoomType, checkInDate, checkOutDate, numberOfRooms]);

    const fetchRoomTypes = async () => {
        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${hotelId}/rooms`);
            if (!response.ok) throw new Error('Failed to fetch room types');

            const data = await response.json();
            setRoomTypes(data);

            // Auto-select first room type if available
            if (data.length > 0) {
                setSelectedRoomType(data[0]);
            }
        } catch (err) {
            console.error('Error fetching room types:', err);
            setError(t('errorLoadingRoomTypes'));
        } finally {
            setLoading(false);
        }
    };

    const fetchAvailability = async () => {
        try {
            // Fetch 90 days from today
            const fromDate = new Date().toISOString().split('T')[0];
            const toDate = new Date();
            toDate.setDate(toDate.getDate() + 90);
            const toDateStr = toDate.toISOString().split('T')[0];

            const response = await fetch(
                `http://localhost:5150/api/hotels/${hotelId}/availability?from=${fromDate}&to=${toDateStr}`
            );

            if (!response.ok) throw new Error('Failed to fetch availability');

            const data = await response.json();
            setAvailability(data);

            // Build set of blocked dates
            const blocked = new Set();
            data.forEach(item => {
                if (item.isBlocked || item.availableCount === 0) {
                    blocked.add(item.date.split('T')[0]);
                }
            });
            setBlockedDates(blocked);
        } catch (err) {
            console.error('Error fetching availability:', err);
        }
    };

    const calculatePrice = async () => {
        if (!selectedRoomType || !checkInDate || !checkOutDate) return;

        try {
            const response = await fetch('http://localhost:5150/api/reservations/calculate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    roomTypeId: selectedRoomType.id,
                    checkInDate,
                    checkOutDate,
                    numberOfRooms
                })
            });

            if (response.ok) {
                const data = await response.json();
                setPriceInfo(data);
            }
        } catch (err) {
            console.error('Error calculating price:', err);
        }
    };

    const handleBooking = async () => {
        if (!user) {
            setError(t('pleaseLoginToBook'));
            return;
        }

        if (!selectedRoomType || !checkInDate || !checkOutDate) {
            setError(t('selectRoomAndDates'));
            return;
        }

        setBookingLoading(true);
        setError('');
        setSuccess('');

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5150/api/reservations', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    hotelId,
                    roomTypeId: selectedRoomType.id,
                    checkInDate,
                    checkOutDate,
                    numberOfRooms
                })
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || 'Грешка при резервация');
            }

            setSuccess(t('bookingSuccessful'));

            // Clear form
            setCheckInDate('');
            setCheckOutDate('');
            setNumberOfRooms(1);
            setPriceInfo(null);

            // Refresh availability
            fetchAvailability();

            // Callback
            if (onBookingComplete) {
                onBookingComplete(data);
            }
        } catch (err) {
            setError(err.message);
        } finally {
            setBookingLoading(false);
        }
    };

    const isDateBlocked = (date) => {
        return blockedDates.has(date);
    };

    const getAvailableRoomsForDate = (date) => {
        if (!selectedRoomType) return 0;
        const record = availability.find(
            a => a.roomTypeId === selectedRoomType.id && a.date.split('T')[0] === date
        );
        return record ? record.availableCount : selectedRoomType.totalRooms;
    };

    if (loading) {
        return (
            <div className="booking-widget loading">
                <div className="spinner"></div>
                <p>{t('loading')}</p>
            </div>
        );
    }

    if (roomTypes.length === 0) {
        return (
            <div className="booking-widget no-rooms">
                <h3>📅 {t('reservation')}</h3>
                <p className="no-rooms-message">
                    {t('noRoomsConfigured')}
                </p>
            </div>
        );
    }

    return (
        <div className="booking-widget">
            <h3>📅 {t('bookNowHeader')}</h3>

            {error && <div className="booking-error">{error}</div>}
            {success && <div className="booking-success">{success}</div>}

            {/* Room Type Selection */}
            <div className="booking-section">
                <label>{t('roomTypeLabel')}</label>
                <div className="room-type-grid">
                    {roomTypes.map(room => (
                        <div
                            key={room.id}
                            className={`room-type-card ${selectedRoomType?.id === room.id ? 'selected' : ''}`}
                            onClick={() => setSelectedRoomType(room)}
                        >
                            <div className="room-type-name">{room.name}</div>
                            <div className="room-type-details">
                                <span className="room-capacity">👥 {room.capacity} {t('guests')}</span>
                                <span className="room-price">{convertAndFormat(room.pricePerNight)}/{t('perNight')}</span>
                            </div>
                            {room.description && (
                                <div className="room-type-description">{room.description}</div>
                            )}
                        </div>
                    ))}
                </div>
            </div>

            {/* Date Selection - Visual Calendar */}
            <div className="booking-section">
                <label>{t('selectDatesLabel')}</label>
                <DateRangeCalendar
                    hotelId={hotelId}
                    selectedRoomType={selectedRoomType}
                    checkInDate={checkInDate}
                    checkOutDate={checkOutDate}
                    onDateChange={(checkIn, checkOut) => {
                        setCheckInDate(checkIn);
                        setCheckOutDate(checkOut);
                    }}
                    availability={availability}
                />
            </div>

            {/* Number of Rooms */}
            {selectedRoomType && (
                <div className="booking-section">
                    <label>{t('numberOfRooms')}</label>
                    <div className="rooms-selector">
                        <button
                            type="button"
                            onClick={() => setNumberOfRooms(Math.max(1, numberOfRooms - 1))}
                            disabled={numberOfRooms <= 1}
                        >
                            −
                        </button>
                        <span className="rooms-count">{numberOfRooms}</span>
                        <button
                            type="button"
                            onClick={() => setNumberOfRooms(numberOfRooms + 1)}
                            disabled={numberOfRooms >= selectedRoomType.totalRooms}
                        >
                            +
                        </button>
                    </div>
                    <small className="rooms-available">
                        {t('availableColon')} {selectedRoomType.totalRooms} {t('roomsOfType')}
                    </small>
                </div>
            )}

            {/* Availability Indicator */}
            {checkInDate && checkOutDate && selectedRoomType && (
                <div className="booking-section availability-section">
                    <div className={`availability-badge ${priceInfo ? 'available' : 'checking'}`}>
                        {priceInfo ? `✓ ${t('available')}` : `⏳ ${t('checking')}`}
                    </div>
                </div>
            )}

            {/* Price Summary */}
            {priceInfo && (
                <div className="booking-section price-summary">
                    {priceInfo.totalDiscount > 0 && (
                        <div className="price-row original">
                            <span>{t('originalPrice')}</span>
                            <span className="strikethrough">{convertAndFormat(priceInfo.originalPrice)}</span>
                        </div>
                    )}
                    <div className="price-row">
                        <span>{convertAndFormat(priceInfo.pricePerNight)} × {priceInfo.numberOfNights} {t('nights')}</span>
                        <span>{convertAndFormat(priceInfo.pricePerNight * priceInfo.numberOfNights)}</span>
                    </div>
                    {priceInfo.totalDiscount > 0 && (
                        <div className="price-row discount">
                            <span>💰 {t('youSave')}</span>
                            <span className="savings">-{convertAndFormat(priceInfo.totalDiscount)}</span>
                        </div>
                    )}
                    {numberOfRooms > 1 && (
                        <div className="price-row">
                            <span>× {numberOfRooms} {t('rooms')}</span>
                            <span></span>
                        </div>
                    )}
                    <div className="price-row total">
                        <span>{t('total')}</span>
                        <span className="total-price">{convertAndFormat(priceInfo.totalPrice)}</span>
                    </div>
                </div>
            )}

            {/* Book Button */}
            <button
                className="btn-book"
                onClick={handleBooking}
                disabled={!selectedRoomType || !checkInDate || !checkOutDate || bookingLoading || !user}
            >
                {bookingLoading ? `⏳ ${t('booking')}` : (
                    priceInfo ? `${t('bookFor')} ${convertAndFormat(priceInfo.totalPrice)}` : t('book')
                )}
            </button>

            {!user && (
                <p className="login-reminder">
                    ⚠️ {t('loginToBookWarning')}
                </p>
            )}
        </div>
    );
};

export default BookingWidget;
