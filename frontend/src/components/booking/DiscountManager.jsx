import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { useLanguage } from '../../context/LanguageContext';
import './DiscountManager.css';

const DiscountManager = ({ hotelId, roomTypes }) => {
    const { t, language } = useLanguage();
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

    const fetchDiscounts = React.useCallback(async () => {
        try {
            const data = await api.get(`/hotels/${hotelId}/discounts`);
            setDiscounts(data);
        } catch (err) {
            setError(t('errorLoadingDiscounts'));
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [hotelId, t]);

    useEffect(() => {
        fetchDiscounts();
    }, [fetchDiscounts]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        // Validation
        if (!formData.roomTypeId || !formData.startDate || !formData.endDate || !formData.discountPercentage) {
            setError(t('allFieldsRequired'));
            return;
        }

        if (new Date(formData.endDate) <= new Date(formData.startDate)) {
            setError(t('endDateAfterStartDate'));
            return;
        }

        if (formData.discountPercentage < 1 || formData.discountPercentage > 99) {
            setError(t('discountRangeError'));
            return;
        }

        try {
            if (editingId) {
                await api.put(`/discounts/${editingId}`, formData);
            } else {
                await api.post(`/hotels/${hotelId}/discounts`, formData);
            }

            setSuccess(editingId ? t('discountUpdated') : t('discountCreated'));
            resetForm();
            fetchDiscounts();
        } catch (err) {
            setError(err.message || t('errorSaving'));
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
        if (!window.confirm(t('confirmDeleteDiscount'))) {
            return;
        }

        try {
            await api.delete(`/discounts/${id}`);

            setSuccess(t('discountDeleted'));
            fetchDiscounts();
        } catch (err) {
            setError(t('errorDeletingDiscount'));
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
        return <div className="discount-loading">{t('loading')}</div>;
    }

    return (
        <div className="discount-manager">
            {error && <div className="message error">{error}</div>}
            {success && <div className="message success">{success}</div>}

            <div className="discount-header">
                <h3>💰 {t('manageDiscounts')}</h3>
                <button
                    className="btn-add-discount"
                    onClick={() => setShowForm(!showForm)}
                >
                    {showForm ? `✕ ${t('cancel')}` : `+ ${t('addDiscount')}`}
                </button>
            </div>

            {/* Add/Edit Form */}
            {showForm && (
                <div className="discount-form-container">
                    <form onSubmit={handleSubmit} className="discount-form">
                        <div className="form-row">
                            <div className="form-group">
                                <label>{t('roomType')}</label>
                                <select
                                    value={formData.roomTypeId}
                                    onChange={(e) => setFormData({ ...formData, roomTypeId: e.target.value })}
                                    required
                                >
                                    <option value="">{t('chooseRoom')}</option>
                                    {roomTypes.map(rt => (
                                        <option key={rt.id} value={rt.id}>
                                            {rt.name} - {rt.pricePerNight} {t('perNight')}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div className="form-group">
                                <label>{t('discountPercentage')}</label>
                                <input
                                    type="number"
                                    min="1"
                                    max="99"
                                    value={formData.discountPercentage}
                                    onChange={(e) => setFormData({ ...formData, discountPercentage: e.target.value })}
                                    placeholder={t('discountExample')}
                                    required
                                />
                            </div>
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label>{t('startDate')}</label>
                                <input
                                    type="date"
                                    value={formData.startDate}
                                    onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                                    required
                                />
                            </div>

                            <div className="form-group">
                                <label>{t('endDate')}</label>
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
                                {t('cancel')}
                            </button>
                            <button type="submit" className="btn-save">
                                💾 {editingId ? t('updateProfile') : t('create')}
                            </button>
                        </div>
                    </form>
                </div>
            )}

            {/* Discounts List */}
            <div className="discounts-list">
                {discounts.length === 0 ? (
                    <div className="no-discounts">
                        <p>📭 {t('noDiscounts')}</p>
                        <p className="hint">{t('useButtonToAddDiscount')}</p>
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
                                        {isDiscountActive(discount) && <span className="status-tag active">⚡ {t('activeTag')}</span>}
                                        {isDiscountExpired(discount) && <span className="status-tag expired">⏱️ {t('expiredTag')}</span>}
                                        {!isDiscountActive(discount) && !isDiscountExpired(discount) && (
                                            <span className="status-tag upcoming">📅 {t('upcomingTag')}</span>
                                        )}
                                    </div>
                                </div>

                                <div className="discount-info">
                                    <h4>{discount.roomTypeName}</h4>
                                    <div className="discount-dates">
                                        <p>
                                            <strong>{t('from')}:</strong> {new Date(discount.startDate).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')}
                                        </p>
                                        <p>
                                            <strong>{t('to')}:</strong> {new Date(discount.endDate).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')}
                                        </p>
                                    </div>
                                </div>

                                <div className="discount-actions">
                                    <button
                                        className="btn-edit-small"
                                        onClick={() => handleEdit(discount)}
                                        title={t('edit')}
                                    >
                                        ✏️
                                    </button>
                                    <button
                                        className="btn-delete-small"
                                        onClick={() => handleDelete(discount.id)}
                                        title={t('delete')}
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
