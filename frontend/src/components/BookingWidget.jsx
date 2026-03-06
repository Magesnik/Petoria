import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import { useCurrency } from '../context/CurrencyContext';
import { useCart } from '../context/CartContext';
import DateRangeCalendar from './DateRangeCalendar';
import AddToCartDialog from './AddToCartDialog';
import './BookingWidget.css';

const BookingWidget = ({ hotelId, hotelName, hotelImage, onBookingComplete }) => {
    const { user } = useAuth();
    const { t } = useLanguage();
    const { convertAndFormat } = useCurrency();
    const { addToCart } = useCart();

    // State for room types
    const [roomTypes, setRoomTypes] = useState([]);
    const [selectedRoomType, setSelectedRoomType] = useState(null);

    // State for dates
    const [checkInDate, setCheckInDate] = useState('');
    const [checkOutDate, setCheckOutDate] = useState('');
    const [numberOfRooms, setNumberOfRooms] = useState(1);

    // State for availability
    const [availability, setAvailability] = useState([]);

    // State for price calculation
    const [priceInfo, setPriceInfo] = useState(null);

    // State for loading and errors
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [unavailableReason, setUnavailableReason] = useState('');

    // Add to cart dialog
    const [cartDialogItem, setCartDialogItem] = useState(null);

    // Use local date string to avoid UTC timezone shifting (e.g. UTC+2 shifts dates back 1 day)
    const toLocalDateStr = (date) => {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    };

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
            // Validate availability: check every day in the range
            const checkRange = () => {
                const [y1, m1, d1] = checkInDate.split('-').map(Number);
                const [y2, m2, d2] = checkOutDate.split('-').map(Number);
                const start = new Date(y1, m1 - 1, d1);
                const end = new Date(y2, m2 - 1, d2);
                for (let d = new Date(start); d < end; d.setDate(d.getDate() + 1)) {
                    const dateStr = toLocalDateStr(d);
                    const record = availability.find(
                        a => a.roomTypeId === selectedRoomType.id && a.date.split('T')[0] === dateStr
                    );
                    if (record) {
                        if (record.isBlocked) return `Датата ${dateStr} е блокирана.`;
                        if (record.availableCount === 0) return `На ${dateStr} няма налични стаи.`;
                        if (record.availableCount < numberOfRooms) return `На ${dateStr} има само ${record.availableCount} стая/и (искате ${numberOfRooms}).`;
                    }
                }
                return '';
            };
            const reason = checkRange();
            setUnavailableReason(reason);
            if (!reason) {
                calculatePrice();
            } else {
                setPriceInfo(null);
            }
        } else {
            setPriceInfo(null);
            setUnavailableReason('');
        }
    }, [selectedRoomType, checkInDate, checkOutDate, numberOfRooms, availability]);

    const fetchRoomTypes = async () => {
        try {
            const data = await api.get(`/hotels/${hotelId}/rooms`);
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
            const fromDate = toLocalDateStr(new Date());
            const toDate = new Date();
            toDate.setDate(toDate.getDate() + 90);
            const toDateStr = toLocalDateStr(toDate);

            const data = await api.get(
                `/hotels/${hotelId}/availability?from=${fromDate}&to=${toDateStr}`
            );

            setAvailability(data);
        } catch (err) {
            console.error('Error fetching availability:', err);
        }
    };

    const calculatePrice = async () => {
        if (!selectedRoomType || !checkInDate || !checkOutDate) return;

        try {
            const data = await api.post('/reservations/calculate', {
                roomTypeId: selectedRoomType.id,
                checkInDate,
                checkOutDate,
                numberOfRooms
            });

            setPriceInfo(data);
        } catch (err) {
            console.error('Error calculating price:', err);
        }
    };

    const handleAddToCart = async () => {
        if (!user) {
            setError(t('pleaseLoginToBook'));
            return;
        }

        if (!selectedRoomType || !checkInDate || !checkOutDate) {
            setError(t('selectRoomAndDates'));
            return;
        }

        setError('');

        const cartItem = {
            hotelId,
            hotelName,
            hotelImage,
            roomTypeId: selectedRoomType.id,
            roomTypeName: selectedRoomType.name,
            checkInDate,
            checkOutDate,
            numberOfRooms,
            priceInfo,
        };

        try {
            await addToCart(cartItem);
            setCartDialogItem(cartItem);

            // Reset form
            setCheckInDate('');
            setCheckOutDate('');
            setNumberOfRooms(1);
            setPriceInfo(null);

            if (onBookingComplete) {
                onBookingComplete(cartItem);
            }
        } catch (err) {
            setError(err.message || t('checkoutError'));
        }
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
        <>
            <div className="booking-widget">
                <h3>🛒 {t('addToCartHeader')}</h3>

                {error && <div className="booking-error">{error}</div>}

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
                    </div>
                )}

                {/* Availability Indicator */}
                {checkInDate && checkOutDate && selectedRoomType && (
                    <div className="booking-section availability-section">
                        {unavailableReason ? (
                            <div className="availability-badge unavailable">
                                ⛔ {unavailableReason}
                            </div>
                        ) : priceInfo ? (
                            <div className="availability-badge available">
                                ✓ {t('available')}
                            </div>
                        ) : (
                            <div className="availability-badge checking">
                                ⏳ {t('checking')}
                            </div>
                        )}
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

                {/* Add to Cart Button */}
                <button
                    className="btn-book"
                    onClick={handleAddToCart}
                    disabled={!selectedRoomType || !checkInDate || !checkOutDate || !user || !!unavailableReason}
                >
                    {priceInfo
                        ? `🛒 ${t('addToCart')} — ${convertAndFormat(priceInfo.totalPrice)}`
                        : `🛒 ${t('addToCart')}`}
                </button>

                {!user && (
                    <p className="login-reminder">
                        ⚠️ {t('loginToBookWarning')}
                    </p>
                )}
            </div>

            {/* Add to Cart Dialog */}
            {cartDialogItem && (
                <AddToCartDialog
                    item={cartDialogItem}
                    onClose={() => setCartDialogItem(null)}
                />
            )}
        </>
    );
};

export default BookingWidget;
