import React, { useState, useEffect } from 'react';
import './DiscountManager.css';

const DiscountManager = ({ hotelId, roomTypes }) => {
    const [discounts, setDiscounts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [showForm, setShowForm] = useState(false);
    const [editingId, setEditingId] = useState(null);

    const [formData, setFormData] = useState({
        roomTypeId: '',
        startDate: '',
        endDate: '',
        discountPercentage: ''
    });

    useEffect(() => {
        fetchDiscounts();
    }, [hotelId]);

    const fetchDiscounts = async () => {
        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${hotelId}/discounts`);
            if (!response.ok) throw new Error('Failed to fetch discounts');
            const data = await response.json();
            setDiscounts(data);
        } catch (err) {
            setError('Грешка при зареждане на отстъпки');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        // Validation
        if (!formData.roomTypeId || !formData.startDate || !formData.endDate || !formData.discountPercentage) {
            setError('Всички полета са задължителни');
            return;
        }

        if (new Date(formData.endDate) <= new Date(formData.startDate)) {
            setError('Крайната дата трябва да е след началната');
            return;
        }

        if (formData.discountPercentage < 1 || formData.discountPercentage > 99) {
            setError('Отстъпката трябва да е между 1% и 99%');
            return;
        }

        try {
            const token = localStorage.getItem('token');
            const url = editingId
                ? `http://localhost:5150/api/discounts/${editingId}`
                : `http://localhost:5150/api/hotels/${hotelId}/discounts`;

            const method = editingId ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(formData)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to save discount');
            }

            setSuccess(editingId ? 'Отстъпката е актуализирана!' : 'Отстъпката е създадена!');
            resetForm();
            fetchDiscounts();
        } catch (err) {
            setError(err.message || 'Грешка при запазване');
            console.error(err);
        }
    };

    const handleEdit = (discount) => {
        setEditingId(discount.id);
        setFormData({
            roomTypeId: discount.roomTypeId,
            startDate: new Date(discount.startDate).toISOString().split('T')[0],
            endDate: new Date(discount.endDate).toISOString().split('T')[0],
            discountPercentage: discount.discountPercentage
        });
        setShowForm(true);
    };

    const handleDelete = async (id) => {
        if (!window.confirm('Сигурни ли сте, че искате да изтриете тази отстъпка?')) {
            return;
        }

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5150/api/discounts/${id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Failed to delete');

            setSuccess('Отстъпката е изтрита!');
            fetchDiscounts();
        } catch (err) {
            setError('Грешка при изтриване');
            console.error(err);
        }
    };

    const resetForm = () => {
        setFormData({
            roomTypeId: '',
            startDate: '',
            endDate: '',
            discountPercentage: ''
        });
        setEditingId(null);
        setShowForm(false);
    };

    const isDiscountActive = (discount) => {
        const now = new Date();
        const start = new Date(discount.startDate);
        const end = new Date(discount.endDate);
        return start <= now && end >= now;
    };

    const isDiscountExpired = (discount) => {
        return new Date(discount.endDate) < new Date();
    };

    if (loading) {
        return <div className="discount-loading">Зареждане...</div>;
    }

    return (
        <div className="discount-manager">
            {error && <div className="message error">{error}</div>}
            {success && <div className="message success">{success}</div>}

            <div className="discount-header">
                <h3>💰 Управление на отстъпки</h3>
                <button
                    className="btn-add-discount"
                    onClick={() => setShowForm(!showForm)}
                >
                    {showForm ? '✕ Отказ' : '+ Добави отстъпка'}
                </button>
            </div>

            {/* Add/Edit Form */}
            {showForm && (
                <div className="discount-form-container">
                    <form onSubmit={handleSubmit} className="discount-form">
                        <div className="form-row">
                            <div className="form-group">
                                <label>Тип стая</label>
                                <select
                                    value={formData.roomTypeId}
                                    onChange={(e) => setFormData({ ...formData, roomTypeId: e.target.value })}
                                    required
                                >
                                    <option value="">Избери стая...</option>
                                    {roomTypes.map(rt => (
                                        <option key={rt.id} value={rt.id}>
                                            {rt.name} - {rt.pricePerNight} лв/нощ
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div className="form-group">
                                <label>Отстъпка (%)</label>
                                <input
                                    type="number"
                                    min="1"
                                    max="99"
                                    value={formData.discountPercentage}
                                    onChange={(e) => setFormData({ ...formData, discountPercentage: e.target.value })}
                                    placeholder="напр. 25"
                                    required
                                />
                            </div>
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label>Начална дата</label>
                                <input
                                    type="date"
                                    value={formData.startDate}
                                    onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                                    required
                                />
                            </div>

                            <div className="form-group">
                                <label>Крайна дата</label>
                                <input
                                    type="date"
                                    value={formData.endDate}
                                    onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                                    required
                                />
                            </div>
                        </div>

                        <div className="form-actions">
                            <button type="button" className="btn-cancel" onClick={resetForm}>
                                Отказ
                            </button>
                            <button type="submit" className="btn-save">
                                💾 {editingId ? 'Актуализирай' : 'Създай'}
                            </button>
                        </div>
                    </form>
                </div>
            )}

            {/* Discounts List */}
            <div className="discounts-list">
                {discounts.length === 0 ? (
                    <div className="no-discounts">
                        <p>📭 Няма създадени отстъпки</p>
                        <p className="hint">Използвайте бутона по-горе за да добавите отстъпка</p>
                    </div>
                ) : (
                    <div className="discounts-grid">
                        {discounts.map(discount => (
                            <div
                                key={discount.id}
                                className={`discount-card ${isDiscountActive(discount) ? 'active' :
                                        isDiscountExpired(discount) ? 'expired' : 'upcoming'
                                    }`}
                            >
                                <div className="discount-card-header">
                                    <div className="discount-badge">
                                        -{discount.discountPercentage}%
                                    </div>
                                    <div className="discount-status">
                                        {isDiscountActive(discount) && <span className="status-tag active">⚡ Активна</span>}
                                        {isDiscountExpired(discount) && <span className="status-tag expired">⏱️ Изтекла</span>}
                                        {!isDiscountActive(discount) && !isDiscountExpired(discount) && (
                                            <span className="status-tag upcoming">📅 Предстояща</span>
                                        )}
                                    </div>
                                </div>

                                <div className="discount-info">
                                    <h4>{discount.roomTypeName}</h4>
                                    <div className="discount-dates">
                                        <p>
                                            <strong>От:</strong> {new Date(discount.startDate).toLocaleDateString('bg-BG')}
                                        </p>
                                        <p>
                                            <strong>До:</strong> {new Date(discount.endDate).toLocaleDateString('bg-BG')}
                                        </p>
                                    </div>
                                </div>

                                <div className="discount-actions">
                                    <button
                                        className="btn-edit-small"
                                        onClick={() => handleEdit(discount)}
                                        title="Редактирай"
                                    >
                                        ✏️
                                    </button>
                                    <button
                                        className="btn-delete-small"
                                        onClick={() => handleDelete(discount.id)}
                                        title="Изтрий"
                                    >
                                        🗑️
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

export default DiscountManager;
