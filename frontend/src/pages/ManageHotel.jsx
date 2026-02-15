import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import RoomTypeManager from '../components/RoomTypeManager';
import AvailabilityCalendar from '../components/AvailabilityCalendar';
import './ManageHotel.css';

const ManageHotel = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user, isAdmin } = useAuth();
    const { t } = useLanguage();

    const [hotel, setHotel] = useState(null);
    const [roomTypes, setRoomTypes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [activeTab, setActiveTab] = useState('rooms');

    // Edit mode
    const [isEditing, setIsEditing] = useState(false);
    const [editData, setEditData] = useState({});

    useEffect(() => {
        if (!isAdmin()) {
            navigate('/');
            return;
        }
        fetchHotel();
    }, [id, isAdmin, navigate]);

    const fetchHotel = async () => {
        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${id}`);
            if (!response.ok) throw new Error('Hotel not found');

            const data = await response.json();

            // Check if user owns this hotel
            console.log('DEBUG - User ID:', user?.id);
            console.log('DEBUG - Hotel createdById:', data.createdById);
            console.log('DEBUG - User roles:', user?.roles);
            console.log('DEBUG - Match:', data.createdById === user?.id);

            if (data.createdById !== user?.id && !user?.roles?.includes('SuperAdmin')) {
                navigate('/my-hotels');
                return;
            }

            setHotel(data);
            setEditData({
                name: data.name,
                description: data.description,
                location: data.location,
                city: data.city,
                country: data.country,
                isAvailable: data.isAvailable
            });
        } catch (err) {
            setError(t('errorLoadingHotel'));
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const fetchRoomTypes = async () => {
        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${id}/rooms`);
            if (response.ok) {
                const data = await response.json();
                setRoomTypes(data);
            }
        } catch (err) {
            console.error('Error fetching room types:', err);
        }
    };

    useEffect(() => {
        if (hotel) {
            fetchRoomTypes();
        }
    }, [hotel]);

    const handleSaveEdit = async () => {
        setError('');
        setSuccess('');

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5150/api/hotels/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    ...hotel,
                    ...editData
                })
            });

            if (!response.ok) throw new Error('Failed to update');

            setSuccess(t('savedSuccessfully'));
            setIsEditing(false);
            fetchHotel();
        } catch (err) {
            setError(t('errorSaving'));
        }
    };

    const handleRoomsChange = (rooms) => {
        setRoomTypes(rooms);
    };

    if (!isAdmin()) return null;

    if (loading) {
        return (
            <div className="manage-hotel-page">
                <Header />
                <div className="loading-state">
                    <div className="spinner"></div>
                    <p>{t('loading')}</p>
                </div>
            </div>
        );
    }

    if (!hotel) {
        return (
            <div className="manage-hotel-page">
                <Header />
                <div className="error-state">
                    <h2>Хотелът не е намерен</h2>
                    <Link to="/my-hotels" className="btn-back">← Назад</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="manage-hotel-page">
            <Header />

            <div className="manage-container">
                {/* Header */}
                <div className="manage-header">
                    <div className="header-left">
                        <Link to="/my-hotels" className="btn-back">← {t('back')}</Link>
                        <h1>{hotel.name}</h1>
                        <p className="hotel-location-text">📍 {hotel.city}, {hotel.country}</p>
                    </div>
                    <Link to={`/hotel/${id}`} className="btn-preview">
                        👁️ {t('viewPage')}
                    </Link>
                </div>

                {error && <div className="message error">{error}</div>}
                {success && <div className="message success">{success}</div>}

                {/* Tabs */}
                <div className="tabs">
                    <button
                        className={`tab ${activeTab === 'info' ? 'active' : ''}`}
                        onClick={() => setActiveTab('info')}
                    >
                        📋 {t('infoTab')}
                    </button>
                    <button
                        className={`tab ${activeTab === 'rooms' ? 'active' : ''}`}
                        onClick={() => setActiveTab('rooms')}
                    >
                        🛏️ {t('roomsTab')} ({roomTypes.length})
                    </button>
                    <button
                        className={`tab ${activeTab === 'availability' ? 'active' : ''}`}
                        onClick={() => setActiveTab('availability')}
                    >
                        📅 {t('availabilityTab')}
                    </button>
                </div>

                {/* Tab Content */}
                <div className="tab-content">
                    {/* Info Tab */}
                    {activeTab === 'info' && (
                        <div className="info-section">
                            {!isEditing ? (
                                <div className="info-view">
                                    <div className="info-grid">
                                        <div className="info-item">
                                            <label>{t('hotelName')}</label>
                                            <p>{hotel.name}</p>
                                        </div>
                                        <div className="info-item">
                                            <label>{t('city')}</label>
                                            <p>{hotel.city}</p>
                                        </div>
                                        <div className="info-item">
                                            <label>{t('country')}</label>
                                            <p>{hotel.country}</p>
                                        </div>
                                        <div className="info-item">
                                            <label>{t('address')}</label>
                                            <p>{hotel.location}</p>
                                        </div>

                                        <div className="info-item">
                                            <label>{t('status')}</label>
                                            <p className={hotel.isAvailable ? 'status-active' : 'status-inactive'}>
                                                {hotel.isAvailable ? `✓ ${t('active')}` : `○ ${t('inactive')}`}
                                            </p>
                                        </div>
                                        <div className="info-item full-width">
                                            <label>{t('description')}</label>
                                            <p>{hotel.description || t('noDescription')}</p>
                                        </div>
                                    </div>
                                    <button className="btn-edit" onClick={() => setIsEditing(true)}>
                                        ✏️ {t('edit')}
                                    </button>
                                </div>
                            ) : (
                                <div className="info-edit">
                                    <div className="edit-grid">
                                        <div className="form-group">
                                            <label>{t('hotelName')}</label>
                                            <input
                                                type="text"
                                                value={editData.name}
                                                onChange={(e) => setEditData({ ...editData, name: e.target.value })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>{t('city')}</label>
                                            <input
                                                type="text"
                                                value={editData.city}
                                                onChange={(e) => setEditData({ ...editData, city: e.target.value })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>{t('country')}</label>
                                            <input
                                                type="text"
                                                value={editData.country}
                                                onChange={(e) => setEditData({ ...editData, country: e.target.value })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>{t('address')}</label>
                                            <input
                                                type="text"
                                                value={editData.location}
                                                onChange={(e) => setEditData({ ...editData, location: e.target.value })}
                                            />
                                        </div>

                                        <div className="form-group">
                                            <label>{t('status')}</label>
                                            <select
                                                value={editData.isAvailable}
                                                onChange={(e) => setEditData({ ...editData, isAvailable: e.target.value === 'true' })}
                                            >
                                                <option value="true">{t('active')}</option>
                                                <option value="false">{t('inactive')}</option>
                                            </select>
                                        </div>
                                        <div className="form-group full-width">
                                            <label>{t('description')}</label>
                                            <textarea
                                                value={editData.description}
                                                onChange={(e) => setEditData({ ...editData, description: e.target.value })}
                                                rows="4"
                                            />
                                        </div>
                                    </div>
                                    <div className="edit-actions">
                                        <button className="btn-cancel" onClick={() => setIsEditing(false)}>
                                            ❌ {t('cancel')}
                                        </button>
                                        <button className="btn-save" onClick={handleSaveEdit}>
                                            ✅ {t('save')}
                                        </button>
                                    </div>
                                </div>
                            )}
                        </div>
                    )}

                    {/* Rooms Tab */}
                    {activeTab === 'rooms' && (
                        <RoomTypeManager
                            hotelId={parseInt(id)}
                            onRoomsChange={handleRoomsChange}
                        />
                    )}

                    {/* Availability Tab */}
                    {activeTab === 'availability' && (
                        <AvailabilityCalendar
                            hotelId={parseInt(id)}
                            roomTypes={roomTypes}
                        />
                    )}
                </div>
            </div>
        </div>
    );
};

export default ManageHotel;
