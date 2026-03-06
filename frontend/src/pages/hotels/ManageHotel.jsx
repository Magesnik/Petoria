import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';

import RoomTypeManager from '../../components/RoomTypeManager';
import DiscountManager from '../../components/DiscountManager';
import PromoCodeManager from '../../components/PromoCodeManager';
import AvailabilityCalendar from '../../components/AvailabilityCalendar';
import './ManageHotel.css';

const ManageHotel = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user, isAdmin, isSuperAdmin } = useAuth();
    const { t } = useLanguage();

    const [hotel, setHotel] = useState(null);
    const [roomTypes, setRoomTypes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [activeTab, setActiveTab] = useState('rooms');

    // Moderator management
    const [moderators, setModerators] = useState([]);
    const [moderatorEmail, setModeratorEmail] = useState('');
    const [showAddModerator, setShowAddModerator] = useState(false);

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
            const data = await api.get(`/hotels/${id}`);

            // Check if user owns this hotel or is moderator
            if (data.createdById !== user?.id && !data.isModerator && !user?.roles?.includes('SuperAdmin')) {
                navigate('/my-hotels');
                return;
            }

            // Block access if hotel is suspended by SuperAdmin (only SuperAdmin can still access it)
            if (data.isSuspendedBySuperAdmin && !isSuperAdmin()) {
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
                isAvailable: data.isAvailable,
                starRating: data.starRating || 3,
                cancellationPolicies: data.cancellationPolicies ? JSON.parse(data.cancellationPolicies) : []
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
            const data = await api.get(`/hotels/${id}/rooms`);
            setRoomTypes(data);
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
            await api.put(`/hotels/${id}`, {
                ...hotel,
                ...editData,
                cancellationPolicies: JSON.stringify(editData.cancellationPolicies)
            });

            setSuccess(t('savedSuccessfully'));
            setIsEditing(false);
            fetchHotel();
        } catch {
            setError(t('errorSaving'));
        }
    };

    const handleRoomsChange = (rooms) => {
        setRoomTypes(rooms);
    };

    const fetchModerators = async () => {
        try {
            const data = await api.get(`/hotels/${id}/moderators`);
            setModerators(data);
        } catch (err) {
            console.error('Error fetching moderators:', err);
        }
    };

    const handleAddModerator = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        try {
            await api.post(`/hotels/${id}/moderators`, { email: moderatorEmail });
            setSuccess(t('moderatorAdded') || 'Модераторът е добавен успешно');
            setModeratorEmail('');
            setShowAddModerator(false);
            fetchModerators();
        } catch (err) {
            if (err.message === 'User with this email not found') {
                setError(t('userNotFound'));
            } else {
                setError(err.message || t('errorAddingModerator') || 'Грешка при добавяне на модератор');
            }
        }
    };

    const handleRemoveModerator = async (userId) => {
        if (!window.confirm(t('confirmRemoveModerator') || 'Сигурни ли сте, че искате да премахнете този модератор?')) {
            return;
        }

        try {
            await api.delete(`/hotels/${id}/moderators/${userId}`);
            setSuccess(t('moderatorRemoved') || 'Модераторът е премахнат успешно');
            fetchModerators();
        } catch {
            setError(t('errorRemovingModerator') || 'Грешка при премахване на модератор');
        }
    };

    useEffect(() => {
        if (activeTab === 'moderators') {
            fetchModerators();
        }
    }, [activeTab]);

    if (!isAdmin()) return null;

    if (loading) {
        return (
            <div className="manage-hotel-page">

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
                <div className="error-state">
                    <h2>{t('hotelNotFound') || 'Хотелът не е намерен'}</h2>
                    <p>Хотелът може да е бил деактивиран от администратор или не съществува.</p>
                    <Link to="/my-hotels" className="btn-back">← {t('back') || 'Назад'}</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="manage-hotel-page">


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

                {/* Inactivity Warning */}
                {!hotel.isAvailable && roomTypes.length === 0 && (
                    <div className="message warning">
                        ⚠️ Хотелът е неактивен. Моля, добавете поне един тип стая в таб "Стаи", за да можете да го активирате.
                    </div>
                )}

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
                    {(hotel.createdById === user?.id || user?.roles?.includes('SuperAdmin')) && (
                        <>
                            <button
                                className={`tab ${activeTab === 'moderators' ? 'active' : ''}`}
                                onClick={() => setActiveTab('moderators')}
                            >
                                🛡️ {t('moderatorsTab') || 'Модератори'}
                            </button>
                            <button
                                className={`tab ${activeTab === 'promocodes' ? 'active' : ''}`}
                                onClick={() => setActiveTab('promocodes')}
                            >
                                🎟️ {t('promoCodesTab') || 'Промо кодове'}
                            </button>
                        </>
                    )}
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

                                        <div className="info-item full-width">
                                            <label>{t('cancellationPolicies') || 'Политики за отмяна'}</label>
                                            {hotel.cancellationPolicies && JSON.parse(hotel.cancellationPolicies).length > 0 ? (
                                                <ul className="policies-list">
                                                    {JSON.parse(hotel.cancellationPolicies)
                                                        .sort((a, b) => b.daysBefore - a.daysBefore)
                                                        .map((policy, index) => (
                                                            <li key={index}>
                                                                До {policy.daysBefore} дни преди настаняване: <strong>{policy.refundPercentage}% възстановяване</strong>
                                                            </li>
                                                        ))}
                                                </ul>
                                            ) : (
                                                <p>{t('noCancellationPolicies') || 'Няма зададени правила за отмяна (безплатно до последния момент)'}</p>
                                            )}
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

                                        <div className="form-group">
                                            <label>{t('starRating') || 'Звезди'}</label>
                                            <select
                                                value={editData.starRating}
                                                onChange={(e) => setEditData({ ...editData, starRating: parseInt(e.target.value) })}
                                            >
                                                <option value="1">1 ⭐</option>
                                                <option value="2">2 ⭐⭐</option>
                                                <option value="3">3 ⭐⭐⭐</option>
                                                <option value="4">4 ⭐⭐⭐⭐</option>
                                                <option value="5">5 ⭐⭐⭐⭐⭐</option>
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

                                        <div className="form-group full-width">
                                            <label>{t('cancellationPolicies') || 'Политики за отмяна'}</label>
                                            <div className="dynamic-list">
                                                {editData.cancellationPolicies.map((policy, index) => (
                                                    <div key={index} className="dynamic-item policy-item" style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '10px' }}>
                                                        <span>До</span>
                                                        <input
                                                            type="number"
                                                            min="0"
                                                            value={policy.daysBefore}
                                                            onChange={(e) => {
                                                                const newPolicies = [...editData.cancellationPolicies];
                                                                const val = parseInt(e.target.value);
                                                                newPolicies[index].daysBefore = isNaN(val) ? 0 : val;
                                                                setEditData({ ...editData, cancellationPolicies: newPolicies });
                                                            }}
                                                            style={{ width: '70px', padding: '5px' }}
                                                        />
                                                        <span>дни:</span>
                                                        <input
                                                            type="number"
                                                            min="0"
                                                            max="100"
                                                            value={policy.refundPercentage}
                                                            onChange={(e) => {
                                                                const newPolicies = [...editData.cancellationPolicies];
                                                                let val = parseInt(e.target.value);
                                                                if (isNaN(val)) val = 0;
                                                                if (val > 100) val = 100;
                                                                newPolicies[index].refundPercentage = val;
                                                                setEditData({ ...editData, cancellationPolicies: newPolicies });
                                                            }}
                                                            style={{ width: '70px', padding: '5px' }}
                                                        />
                                                        <span>% възстановяване</span>
                                                        <button
                                                            type="button"
                                                            className="btn-remove"
                                                            onClick={() => {
                                                                const newPolicies = editData.cancellationPolicies.filter((_, i) => i !== index);
                                                                setEditData({ ...editData, cancellationPolicies: newPolicies });
                                                            }}
                                                            style={{ marginLeft: 'auto' }}
                                                        >
                                                            ✕
                                                        </button>
                                                    </div>
                                                ))}
                                            </div>
                                            <button
                                                type="button"
                                                className="btn-add"
                                                onClick={() => {
                                                    setEditData({
                                                        ...editData,
                                                        cancellationPolicies: [...editData.cancellationPolicies, { daysBefore: 7, refundPercentage: 100 }]
                                                    });
                                                }}
                                                style={{ marginTop: '10px' }}
                                            >
                                                ➕ {t('addCancellationPolicy') || 'Добави правило'}
                                            </button>
                                            <p className="hint-text" style={{ fontSize: '0.85em', color: '#666', marginTop: '5px' }}>
                                                Пример: 60 дни преди настаняване -&gt; 100% възстановяване на сумата. За липсващи дни до самата дата на настаняване (0 дни) се приема 0% (без възстановяване). Ако нямате въведени правила, приемаме, че отмяната е винаги 100% безплатна.
                                            </p>
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

                    {/* Moderators Tab */}
                    {activeTab === 'moderators' && (
                        <div className="moderators-section">
                            <div className="moderators-header">
                                <h3>{t('manageModerators') || 'Управление на модератори'}</h3>
                                <button
                                    className="btn-add"
                                    onClick={() => setShowAddModerator(!showAddModerator)}
                                >
                                    {showAddModerator ? `❌ ${t('cancel')}` : `➕ ${t('addModerator') || 'Добави модератор'}`}
                                </button>
                            </div>

                            {showAddModerator && (
                                <div className="add-moderator-form">
                                    <form onSubmit={handleAddModerator}>
                                        <div className="form-group">
                                            <label>{t('userEmail')}</label>
                                            <input
                                                type="email"
                                                value={moderatorEmail}
                                                onChange={(e) => setModeratorEmail(e.target.value)}
                                                placeholder="user@example.com"
                                                required
                                            />
                                        </div>
                                        <button type="submit" className="btn-save">
                                            ✅ {t('add')}
                                        </button>
                                    </form>
                                </div>
                            )}

                            <div className="moderators-list">
                                {moderators.length === 0 ? (
                                    <p className="no-data">{t('noModerators') || 'Няма добавени модератори за този хотел.'}</p>
                                ) : (
                                    <div className="moderators-grid">
                                        {moderators.map(mod => (
                                            <div key={mod.userId} className="moderator-card">
                                                <div className="moderator-info">
                                                    <strong>{mod.firstName} {mod.lastName}</strong>
                                                    <span>{mod.email}</span>
                                                    <small>{t('addedOn') || 'Добавен на'}: {new Date(mod.addedAt).toLocaleDateString()}</small>
                                                </div>
                                                <button
                                                    className="btn-delete"
                                                    onClick={() => handleRemoveModerator(mod.userId)}
                                                    title={t('remove')}
                                                >
                                                    ❌ {t('remove')}
                                                </button>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>

                        </div>
                    )}

                    {/* Promo Codes Tab */}
                    {activeTab === 'promocodes' && (
                        <div className="promocodes-section">
                            <PromoCodeManager hotelId={parseInt(id)} />
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ManageHotel;
