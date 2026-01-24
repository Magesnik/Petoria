import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Header from '../components/Header';
import RoomTypeManager from '../components/RoomTypeManager';
import AvailabilityCalendar from '../components/AvailabilityCalendar';
import './ManageHotel.css';

const ManageHotel = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user, isAdmin } = useAuth();

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
                pricePerNight: data.pricePerNight,
                isAvailable: data.isAvailable
            });
        } catch (err) {
            setError('Грешка при зареждане на хотела');
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

            setSuccess('Хотелът е обновен успешно!');
            setIsEditing(false);
            fetchHotel();
        } catch (err) {
            setError('Грешка при запазване');
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
                    <p>Зареждане...</p>
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
                        <Link to="/my-hotels" className="btn-back">← Назад</Link>
                        <h1>{hotel.name}</h1>
                        <p className="hotel-location-text">📍 {hotel.city}, {hotel.country}</p>
                    </div>
                    <Link to={`/hotel/${id}`} className="btn-preview">
                        👁️ Преглед на страницата
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
                        📋 Информация
                    </button>
                    <button
                        className={`tab ${activeTab === 'rooms' ? 'active' : ''}`}
                        onClick={() => setActiveTab('rooms')}
                    >
                        🛏️ Стаи ({roomTypes.length})
                    </button>
                    <button
                        className={`tab ${activeTab === 'availability' ? 'active' : ''}`}
                        onClick={() => setActiveTab('availability')}
                    >
                        📅 Наличност
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
                                            <label>Име на хотела</label>
                                            <p>{hotel.name}</p>
                                        </div>
                                        <div className="info-item">
                                            <label>Град</label>
                                            <p>{hotel.city}</p>
                                        </div>
                                        <div className="info-item">
                                            <label>Държава</label>
                                            <p>{hotel.country}</p>
                                        </div>
                                        <div className="info-item">
                                            <label>Адрес</label>
                                            <p>{hotel.location}</p>
                                        </div>
                                        <div className="info-item">
                                            <label>Базова цена</label>
                                            <p>{hotel.pricePerNight} лв/нощ</p>
                                        </div>
                                        <div className="info-item">
                                            <label>Статус</label>
                                            <p className={hotel.isAvailable ? 'status-active' : 'status-inactive'}>
                                                {hotel.isAvailable ? '✓ Активен' : '○ Неактивен'}
                                            </p>
                                        </div>
                                        <div className="info-item full-width">
                                            <label>Описание</label>
                                            <p>{hotel.description || 'Няма описание'}</p>
                                        </div>
                                    </div>
                                    <button className="btn-edit" onClick={() => setIsEditing(true)}>
                                        ✏️ Редактирай
                                    </button>
                                </div>
                            ) : (
                                <div className="info-edit">
                                    <div className="edit-grid">
                                        <div className="form-group">
                                            <label>Име на хотела</label>
                                            <input
                                                type="text"
                                                value={editData.name}
                                                onChange={(e) => setEditData({ ...editData, name: e.target.value })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>Град</label>
                                            <input
                                                type="text"
                                                value={editData.city}
                                                onChange={(e) => setEditData({ ...editData, city: e.target.value })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>Държава</label>
                                            <input
                                                type="text"
                                                value={editData.country}
                                                onChange={(e) => setEditData({ ...editData, country: e.target.value })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>Адрес</label>
                                            <input
                                                type="text"
                                                value={editData.location}
                                                onChange={(e) => setEditData({ ...editData, location: e.target.value })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>Базова цена (лв)</label>
                                            <input
                                                type="number"
                                                value={editData.pricePerNight}
                                                onChange={(e) => setEditData({ ...editData, pricePerNight: parseFloat(e.target.value) })}
                                            />
                                        </div>
                                        <div className="form-group">
                                            <label>Статус</label>
                                            <select
                                                value={editData.isAvailable}
                                                onChange={(e) => setEditData({ ...editData, isAvailable: e.target.value === 'true' })}
                                            >
                                                <option value="true">Активен</option>
                                                <option value="false">Неактивен</option>
                                            </select>
                                        </div>
                                        <div className="form-group full-width">
                                            <label>Описание</label>
                                            <textarea
                                                value={editData.description}
                                                onChange={(e) => setEditData({ ...editData, description: e.target.value })}
                                                rows="4"
                                            />
                                        </div>
                                    </div>
                                    <div className="edit-actions">
                                        <button className="btn-cancel" onClick={() => setIsEditing(false)}>
                                            Отказ
                                        </button>
                                        <button className="btn-save" onClick={handleSaveEdit}>
                                            💾 Запази
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
