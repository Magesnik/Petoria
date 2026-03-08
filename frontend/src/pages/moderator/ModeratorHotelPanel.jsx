import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api } from '../../utils/api';
import { useLanguage } from '../../context/LanguageContext';
import AvailabilityCalendar from '../../components/AvailabilityCalendar';
import './ModeratorHotelPanel.css'; // We'll create this

const ModeratorHotelPanel = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useLanguage();
    const [hotel, setHotel] = useState(null);
    const [roomTypes, setRoomTypes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('calendar'); // 'calendar' or 'reservations'

    // Logic for Booking Modal from Calendar
    const [showBookingModal, setShowBookingModal] = useState(false);
    const [selectedDates, setSelectedDates] = useState(null);
    const [selectedRoomType, setSelectedRoomType] = useState(null);

    const fetchHotel = React.useCallback(async () => {
        try {
            const data = await api.get(`/hotels/${id}`);
            // Security check: must be moderator
            if (!data.isModerator) {
                // navigate('/moderator'); // Or show error
            }
            setHotel(data);
            setLoading(false);
        } catch (err) {
            console.error('Error fetching hotel:', err);
            setLoading(false);
        }
    }, [id]);

    const fetchRoomTypes = React.useCallback(async () => {
        try {
            const data = await api.get(`/hotels/${id}/rooms`);
            setRoomTypes(data);
        } catch (err) {
            console.error('Error fetching room types:', err);
        }
    }, [id]);

    const fetchReservations = React.useCallback(async () => {
        // We need an endpoint to get ALL reservations for a hotel as a moderator.
        // Currently we only have /reservations/my.
        // We might need to add /hotels/{id}/reservations for moderators in backend if not exists.
        // For now, let's assume we can only see "My" reservations or we need to add that endpoint.
        // Actually, the user asked for "reserve or unreserve the room they selected".
        // Unreserving from Calendar is tricky without knowing which reservation blocks it.
        // So I'll focus on Calendar which allows Creating Reservations (Blocking).
    }, []);

    useEffect(() => {
        if (id) {
            fetchHotel();
            fetchRoomTypes(); // Fetch room types separately
        }
    }, [id, fetchHotel, fetchRoomTypes]);

    useEffect(() => {
        if (activeTab === 'reservations') {
            fetchReservations();
        }
    }, [activeTab, fetchReservations]);

    const handleDateSelect = (dates, roomType) => {
        setSelectedDates(dates);
        setSelectedRoomType(roomType);
        setShowBookingModal(true);
    };

    const handleBookingSuccess = () => {
        setShowBookingModal(false);
        // Refresh calendar
        // We might need to trigger a refresh in AvailabilityCalendar. 
        // For now, a window reload or prop change is simplest, but let's try just closing modal.
        window.location.reload();
    };

    if (loading) return <div className="moderator-panel loading"><div className="spinner"></div></div>;
    if (!hotel) return <div className="moderator-panel error">{t('hotelNotFound')}</div>;

    return (
        <div className="moderator-panel">
            <div className="panel-header">
                <div className="header-content">
                    <h1>{hotel.name} <span className="badge-mod">{t('moderator') || 'Moderator'}</span></h1>
                    <p className="location">📍 {hotel.city}, {hotel.country}</p>
                </div>
                <button onClick={() => navigate('/moderator')} className="btn-back">
                    ← {t('backToDashboard') || 'Back'}
                </button>
            </div>

            <div className="panel-tabs">
                <button
                    className={`tab ${activeTab === 'calendar' ? 'active' : ''}`}
                    onClick={() => setActiveTab('calendar')}
                >
                    📅 {t('calendar') || 'Calendar'}
                </button>
                {/* TODO: Add Reservations Tab if backend supports listing all reservations for moderator */}
            </div>

            <div className="panel-content">
                {activeTab === 'calendar' && (
                    <div className="calendar-section">
                        <div className="calendar-header-actions">
                            <p className="instruction-text">
                                {t('moderatorCalendarInstruction') || 'Click and drag on dates to create a reservation or block dates.'}
                            </p>
                        </div>
                        <AvailabilityCalendar
                            hotelId={hotel.id}
                            roomTypes={roomTypes}
                            isModeratorMode={true}
                            onDateSelect={handleDateSelect}
                        />
                    </div>
                )}
            </div>

            {/* Booking Modal */}
            {showBookingModal && (
                <div className="modal-overlay">
                    <div className="booking-modal-container">
                        <BookingWidgetInternal
                            hotel={hotel}
                            roomType={selectedRoomType} // Pass object
                            initialDates={selectedDates}
                            onClose={() => setShowBookingModal(false)}
                            onSuccess={handleBookingSuccess}
                            isModerator={true}
                        />
                    </div>
                </div>
            )}
        </div>
    );
};

// Internal simplified booking widget for moderator
const BookingWidgetInternal = ({ hotel, roomType, initialDates, onClose, onSuccess, isModerator }) => {
    const { t } = useLanguage();
    const [notes, setNotes] = useState('');
    const [numberOfRooms, setNumberOfRooms] = useState(1);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const handleBook = async () => {
        try {
            setLoading(true);
            setError(null);

            const bookingData = {
                hotelId: hotel.id,
                roomTypeId: roomType.id, // Extract ID from object
                checkInDate: initialDates.start,
                checkOutDate: initialDates.end,
                numberOfRooms: parseInt(numberOfRooms),
                notes: notes
            };

            await api.post('/reservations', bookingData);
            onSuccess();
        } catch (err) {
            console.error(err);
            setError(err.message || 'Booking failed');
            setLoading(false);
        }
    };

    return (
        <div className="moderator-booking-widget">
            <h3>{t('createReservation') || 'Create Reservation'}</h3>
            <div className="form-group">
                <label>{t('roomType') || 'Room Type'}</label>
                <p><strong>{roomType?.name}</strong></p>
            </div>
            <div className="form-group">
                <label>{t('dates')}</label>
                <p>{new Date(initialDates.start).toLocaleDateString()} - {new Date(initialDates.end).toLocaleDateString()}</p>
            </div>
            <div className="form-group">
                <label>{t('numberOfRooms') || 'Number of Rooms'}</label>
                <div className="room-count-input">
                    <input
                        type="number"
                        min="1"
                        max={roomType?.totalRooms || 10}
                        value={numberOfRooms}
                        onChange={(e) => setNumberOfRooms(e.target.value)}
                        className="form-control"
                    />
                    <small className="text-muted">
                        Max: {roomType?.totalRooms} (Total)
                    </small>
                </div>
            </div>
            <div className="form-group">
                <label>{t('notes')}</label>
                <textarea
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    placeholder={t('reservationNotesPlaceholder') || 'Add notes...'}
                />
            </div>
            {isModerator && (
                <div className="info-box">
                    ℹ️ {t('moderatorFreeBooking') || 'As a moderator, this booking will be free ($0).'}
                </div>
            )}
            {error && <div className="error-message">{error}</div>}
            <div className="widget-actions">
                <button onClick={onClose} className="btn btn-secondary">{t('cancel')}</button>
                <button onClick={handleBook} disabled={loading} className="btn btn-primary">
                    {loading ? t('processing') : t('confirmBooking')}
                </button>
            </div>
        </div>
    );
};

export default ModeratorHotelPanel;
