import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';
import './RoomTypeManager.css';

const RoomTypeManager = ({ hotelId, onRoomsChange }) => {
    const { t } = useLanguage();
    const [roomTypes, setRoomTypes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    // Form state for adding/editing
    const [isEditing, setIsEditing] = useState(false);
    const [editingId, setEditingId] = useState(null);
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        pricePerNight: '',
        capacity: 2,
        totalRooms: 1,
        imageUrl: ''
    });

    useEffect(() => {
        if (hotelId) {
            fetchRoomTypes();
        }
    }, [hotelId, fetchRoomTypes]);

    const fetchRoomTypes = React.useCallback(async () => {
        try {
            const data = await api.get(`/hotels/${hotelId}/rooms`);
            setRoomTypes(data);

            if (onRoomsChange) {
                onRoomsChange(data);
            }
        } catch (err) {
            console.error('Error fetching room types:', err);
            setError(t('errorLoadingRooms'));
        } finally {
            setLoading(false);
        }
    }, [hotelId, onRoomsChange, t]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        if (!formData.name || !formData.pricePerNight) {
            setError(t('fillNameAndPrice'));
            return;
        }

        try {
            const body = {
                ...formData,
                id: editingId || 0,
                hotelId,
                pricePerNight: parseFloat(formData.pricePerNight),
                capacity: parseInt(formData.capacity),
                totalRooms: parseInt(formData.totalRooms)
            };

            if (editingId) {
                await api.put(`/hotels/${hotelId}/rooms/${editingId}`, body);
            } else {
                await api.post(`/hotels/${hotelId}/rooms`, body);
            }

            setSuccess(editingId ? t('roomUpdated') : t('roomAdded'));
            resetForm();
            fetchRoomTypes();
        } catch (err) {
            setError(err.message || t('errorSaving'));
        }
    };

    const handleEdit = (room) => {
        setIsEditing(true);
        setEditingId(room.id);
        setFormData({
            name: room.name,
            description: room.description || '',
            pricePerNight: room.pricePerNight.toString(),
            capacity: room.capacity,
            totalRooms: room.totalRooms,
            imageUrl: room.imageUrl || ''
        });
    };

    const handleDelete = async (id) => {
        if (!window.confirm(t('confirmDelete'))) {
            return;
        }

        try {
            await api.delete(`/hotels/${hotelId}/rooms/${id}`);

            setSuccess(t('roomDeleted'));
            fetchRoomTypes();
        } catch {
            setError(t('errorDeleting'));
        }
    };

    const resetForm = () => {
        setIsEditing(false);
        setEditingId(null);
        setFormData({
            name: '',
            description: '',
            pricePerNight: '',
            capacity: 2,
            totalRooms: 1,
            imageUrl: ''
        });
    };



    if (loading) {
        return <div className="room-manager loading">{t('loading')}</div>;
    }

    return (
        <div className="room-manager">
            <div className="room-manager-header">
                <h3>🛏️ {t('roomManagement')}</h3>

            </div>

            {error && <div className="room-error">{error}</div>}
            {success && <div className="room-success">{success}</div>}

            {/* Existing Room Types */}
            {roomTypes.length > 0 && (
                <div className="room-list">
                    {roomTypes.map(room => (
                        <div key={room.id} className="room-item">
                            <div className="room-info">
                                <h4>{room.name}</h4>
                                <div className="room-meta">
                                    <span>💰 {room.pricePerNight} лв/нощ</span>
                                    <span>👥 {room.capacity} гости</span>
                                    <span>🚪 {room.totalRooms} стаи</span>
                                </div>
                                {room.description && (
                                    <p className="room-desc">{room.description}</p>
                                )}
                            </div>
                            <div className="room-actions">
                                <button
                                    className="btn-edit"
                                    onClick={() => handleEdit(room)}
                                >
                                    ✏️
                                </button>
                                <button
                                    className="btn-delete"
                                    onClick={() => handleDelete(room.id)}
                                >
                                    🗑️
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {/* Add/Edit Form */}
            <form onSubmit={handleSubmit} className="room-form">
                <h4>{isEditing ? `✏️ ${t('editRoom')}` : `➕ ${t('addNewRoomType')}`}</h4>

                <div className="form-grid">
                    <div className="form-group">
                        <label>{t('roomName')} *</label>
                        <input
                            type="text"
                            value={formData.name}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            placeholder="напр. Двойна стая, Апартамент..."
                            required
                        />
                    </div>

                    <div className="form-group">
                        <label>{t('pricePerNightLabel')} (лв) *</label>
                        <input
                            type="number"
                            value={formData.pricePerNight}
                            onChange={(e) => setFormData({ ...formData, pricePerNight: e.target.value })}
                            placeholder="150"
                            min="0"
                            step="0.01"
                            required
                        />
                    </div>

                    <div className="form-group">
                        <label>{t('capacityLabel')}</label>
                        <input
                            type="number"
                            value={formData.capacity}
                            onChange={(e) => setFormData({ ...formData, capacity: parseInt(e.target.value) || 1 })}
                            min="1"
                            max="20"
                        />
                    </div>

                    <div className="form-group">
                        <label>{t('totalRoomsLabel')}</label>
                        <input
                            type="number"
                            value={formData.totalRooms}
                            onChange={(e) => setFormData({ ...formData, totalRooms: parseInt(e.target.value) || 1 })}
                            min="1"
                            max="1000"
                        />
                    </div>
                </div>

                <div className="form-group full-width">
                    <label>{t('description')}</label>
                    <textarea
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        placeholder="Описание на стаята и удобства..."
                        rows="2"
                    />
                </div>

                <div className="form-actions">
                    {isEditing && (
                        <button type="button" className="btn-cancel" onClick={resetForm}>
                            {t('cancel')}
                        </button>
                    )}
                    <button type="submit" className="btn-save">
                        {isEditing ? `💾 ${t('saveChanges')}` : `➕ ${t('addRoom')}`}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default RoomTypeManager;
