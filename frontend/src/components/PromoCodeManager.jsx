import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';
import './PromoCodeManager.css';

const PromoCodeManager = ({ hotelId }) => {
    const { t } = useLanguage();
    const [promoCodes, setPromoCodes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [isEditing, setIsEditing] = useState(false);
    const [editingId, setEditingId] = useState(null);

    const [formData, setFormData] = useState({
        code: '',
        discountPercentage: '',
        maxActivations: '',
        validDays: '30'
    });

    useEffect(() => {
        if (hotelId) {
            fetchPromoCodes();
        }
    }, [hotelId]);

    const fetchPromoCodes = async () => {
        try {
            setLoading(true);
            const data = await api.get(`/hotels/${hotelId}/promocodes`);
            setPromoCodes(data);
        } catch (err) {
            console.error('Error fetching promo codes:', err);
            setError(t('errorLoadingPromoCodes') || 'Error loading promo codes');
        } finally {
            setLoading(false);
        }
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const generateRandomCode = () => {
        const randomCode = Math.random().toString(36).substring(2, 10).toUpperCase();
        setFormData(prev => ({
            ...prev,
            code: randomCode
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        try {
            const payload = {
                ...formData,
                discountPercentage: parseFloat(formData.discountPercentage),
                maxActivations: parseInt(formData.maxActivations),
                validDays: parseInt(formData.validDays)
            };

            if (isEditing) {
                // Update
                await api.put(`/promocodes/${editingId}`, {
                    discountPercentage: payload.discountPercentage,
                    maxActivations: payload.maxActivations
                });
                setSuccess(t('promoCodeUpdated') || 'Promo code updated successfully');
            } else {
                // Create
                await api.post(`/hotels/${hotelId}/promocodes`, payload);
                setSuccess(t('promoCodeCreated') || 'Promo code created successfully');
            }

            resetForm();
            fetchPromoCodes();
        } catch (err) {
            console.error('Error saving promo code:', err);
            setError(err.message || t('errorSavingPromoCode') || 'Error saving promo code');
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm(t('confirmDeletePromoCode') || 'Are you sure you want to delete this promo code?')) {
            return;
        }

        try {
            await api.delete(`/promocodes/${id}`);
            setSuccess(t('promoCodeDeleted') || 'Promo code deleted successfully');
            fetchPromoCodes();
        } catch (err) {
            console.error('Error deleting promo code:', err);
            setError(t('errorDeletingPromoCode') || 'Error deleting promo code');
        }
    };

    const handleEdit = (promo) => {
        setIsEditing(true);
        setEditingId(promo.id);
        setFormData({
            code: promo.code,
            discountPercentage: promo.discountPercentage,
            maxActivations: promo.maxActivations,
            validDays: '30' // Not editable, just default
        });
    };

    const resetForm = () => {
        setIsEditing(false);
        setEditingId(null);
        setFormData({
            code: '',
            discountPercentage: '',
            maxActivations: '',
            validDays: '30'
        });
        setError('');
        setSuccess('');
    };

    if (loading && promoCodes.length === 0) return <div>{t('loading') || 'Loading...'}</div>;

    return (
        <div className="promo-code-manager">
            <h3>🎟️ {t('promoCodes') || 'Promo Codes'}</h3>

            {error && <div className="message error">{error}</div>}
            {success && <div className="message success">{success}</div>}

            <div className="promo-codes-list">
                {promoCodes.length === 0 ? (
                    <p className="no-items">{t('noPromoCodes') || 'No promo codes found.'}</p>
                ) : (
                    <table className="promo-table">
                        <thead>
                            <tr>
                                <th>{t('code') || 'Code'}</th>
                                <th>{t('discount') || 'Discount'}</th>
                                <th>{t('activations') || 'Activations'}</th>
                                <th>{t('expires') || 'Expires'}</th>
                                <th>{t('status') || 'Status'}</th>
                                <th>{t('actions') || 'Actions'}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {promoCodes.map(promo => (
                                <tr key={promo.id} className={!promo.isActive ? 'expired' : ''}>
                                    <td className="code-cell"><code>{promo.code}</code></td>
                                    <td>{promo.discountPercentage}%</td>
                                    <td>
                                        {promo.currentActivations} / {promo.maxActivations}
                                    </td>
                                    <td>{new Date(promo.expirationDate).toLocaleDateString()}</td>
                                    <td>
                                        <span className={`status-badge ${promo.isActive ? 'active' : 'inactive'}`}>
                                            {promo.isActive ? (t('active') || 'Active') : (t('inactive') || 'Inactive')}
                                        </span>
                                    </td>
                                    <td>
                                        <button
                                            className="btn-icon edit"
                                            onClick={() => handleEdit(promo)}
                                            title={t('edit') || 'Edit'}
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            className="btn-icon delete"
                                            onClick={() => handleDelete(promo.id)}
                                            title={t('delete') || 'Delete'}
                                        >
                                            🗑️
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>

            <form onSubmit={handleSubmit} className="promo-form">
                <h4>{isEditing ? (t('editPromoCode') || 'Edit Promo Code') : (t('createPromoCode') || 'Create New Promo Code')}</h4>

                <div className="form-row">
                    <div className="form-group code-group">
                        <label>{t('code') || 'Code'}</label>
                        <div className="input-with-button">
                            <input
                                type="text"
                                name="code"
                                value={formData.code}
                                onChange={handleInputChange}
                                placeholder="SUMMER2024"
                                required
                                disabled={isEditing}
                            />
                            {!isEditing && (
                                <button
                                    type="button"
                                    className="btn-generate"
                                    onClick={generateRandomCode}
                                    title={t('generateRandom') || 'Generate Random'}
                                >
                                    🎲
                                </button>
                            )}
                        </div>
                    </div>

                    <div className="form-group">
                        <label>{t('discountPercentage') || 'Discount (%)'}</label>
                        <input
                            type="number"
                            name="discountPercentage"
                            value={formData.discountPercentage}
                            onChange={handleInputChange}
                            min="0"
                            max="100"
                            step="0.1"
                            required
                        />
                    </div>
                </div>

                <div className="form-row">
                    <div className="form-group">
                        <label>{t('maxActivations') || 'Max Activations'}</label>
                        <input
                            type="number"
                            name="maxActivations"
                            value={formData.maxActivations}
                            onChange={handleInputChange}
                            min="1"
                            required
                        />
                    </div>

                    {!isEditing && (
                        <div className="form-group">
                            <label>{t('validDays') || 'Valid Days'}</label>
                            <input
                                type="number"
                                name="validDays"
                                value={formData.validDays}
                                onChange={handleInputChange}
                                min="1"
                                required
                            />
                        </div>
                    )}
                </div>

                <div className="form-actions">
                    {isEditing && (
                        <button type="button" className="btn-cancel" onClick={resetForm}>
                            {t('cancel') || 'Cancel'}
                        </button>
                    )}
                    <button type="submit" className="btn-save">
                        {isEditing ? (t('saveChanges') || 'Save Changes') : (t('createPromoCode') || 'Create Promo Code')}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default PromoCodeManager;
