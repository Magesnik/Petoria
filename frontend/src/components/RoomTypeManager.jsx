import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import './RoomTypeManager.css';

const RoomTypeManager = ({ hotelId, onRoomsChange }) => {
    const { user } = useAuth();
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
    }, [hotelId]);

    const fetchRoomTypes = async () => {
        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${hotelId}/rooms`);
            if (!response.ok) throw new Error('Failed to fetch room types');

            const data = await response.json();
            setRoomTypes(data);

            if (onRoomsChange) {
                onRoomsChange(data);
            }
        } catch (err) {
            console.error('Error fetching room types:', err);
            setError('Грешка при зареждане на стаите');
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        if (!formData.name || !formData.pricePerNight) {
            setError('Моля, попълнете име и цена');
            return;
        }

        try {
            const token = localStorage.getItem('token');
            const url = editingId
                ? `http://localhost:5150/api/hotels/${hotelId}/rooms/${editingId}`
                : `http://localhost:5150/api/hotels/${hotelId}/rooms`;

            const response = await fetch(url, {
                method: editingId ? 'PUT' : 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    ...formData,
                    id: editingId || 0,
                    hotelId,
                    pricePerNight: parseFloat(formData.pricePerNight),
                    capacity: parseInt(formData.capacity),
                    totalRooms: parseInt(formData.totalRooms)
                })
            });

            if (!response.ok) {
                const data = await response.json();
                throw new Error(data.message || 'Грешка при запазване');
            }

            setSuccess(editingId ? 'Стаята е обновена успешно!' : 'Стаята е добавена успешно!');
            resetForm();
            fetchRoomTypes();
        } catch (err) {
            setError(err.message);
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
        if (!window.confirm('Сигурни ли сте, че искате да изтриете тази стая?')) {
            return;
        }

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5150/api/hotels/${hotelId}/rooms/${id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Failed to delete');

            setSuccess('Стаята е изтрита успешно!');
            fetchRoomTypes();
        } catch (err) {
            setError('Грешка при изтриване');
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

    const handleInitializeAvailability = async () => {
        try {
            const token = localStorage.getItem('token');
            const response = await fetch(
                `http://localhost:5150/api/hotels/${hotelId}/availability/initialize?daysAhead=90`,
                {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                }
            );

            if (!response.ok) {
                const data = await response.json();
                throw new Error(data.message || 'Грешка');
            }

            setSuccess('Наличността е инициализирана за следващите 90 дни!');
        } catch (err) {
            setError(err.message);
        }
    };

    if (loading) {
        return <div className="room-manager loading">Зареждане...</div>;
    }

    return (
        <div className="room-manager">
            <div className="room-manager-header">
                <h3>🛏️ Управление на стаи</h3>
                {roomTypes.length > 0 && (
                    <button
                        className="btn-initialize"
                        onClick={handleInitializeAvailability}
                    >
                        📅 Инициализирай наличност
                    </button>
                )}
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
                <h4>{isEditing ? '✏️ Редактирай стая' : '➕ Добави нов тип стая'}</h4>

                <div className="form-grid">
                    <div className="form-group">
                        <label>Име на стаята *</label>
                        <input
                            type="text"
                            value={formData.name}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            placeholder="напр. Двойна стая, Апартамент..."
                            required
                        />
                    </div>

                    <div className="form-group">
                        <label>Цена на нощувка (лв) *</label>
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
                        <label>Капацитет (гости)</label>
                        <input
                            type="number"
                            value={formData.capacity}
                            onChange={(e) => setFormData({ ...formData, capacity: parseInt(e.target.value) || 1 })}
                            min="1"
                            max="20"
                        />
                    </div>

                    <div className="form-group">
                        <label>Брой стаи</label>
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
                    <label>Описание</label>
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
                            Отказ
                        </button>
                    )}
                    <button type="submit" className="btn-save">
                        {isEditing ? '💾 Запази промените' : '➕ Добави стая'}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default RoomTypeManager;
