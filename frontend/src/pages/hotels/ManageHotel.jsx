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

const LocationPicker = React.lazy(() => import('../../components/LocationPicker'));

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
    const [images, setImages] = useState([]);
    const [imageFiles, setImageFiles] = useState([]);
    const [imagePreviews, setImagePreviews] = useState([]);
    const [uploading, setUploading] = useState(false);
    const [uploadingImage, setUploadingImage] = useState(false);
    const [hoveredStar, setHoveredStar] = useState(null);

    useEffect(() => {
        if (!isAdmin()) {
            navigate('/');
            return;
        }
        fetchHotel();
    }, [isAdmin, navigate, fetchHotel]);

    const fetchHotel = React.useCallback(async () => {
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
                latitude: data.latitude,
                longitude: data.longitude,
                imageUrl: data.imageUrl,
                isAvailable: data.isAvailable,
                starRating: data.starRating || 3,
                cancellationPolicies: data.cancellationPolicies ? JSON.parse(data.cancellationPolicies) : []
            });

            const additionalImages = data.images ? JSON.parse(data.images) : [];
            setImages(additionalImages);
            setImageFiles(new Array(additionalImages.length).fill(null));
            setImagePreviews(new Array(additionalImages.length).fill(null));
        } catch (err) {
            setError(t('errorLoadingHotel'));
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [id, user, isSuperAdmin, navigate, t]);

    const fetchRoomTypes = React.useCallback(async () => {
        try {
            const data = await api.get(`/hotels/${id}/rooms`);
            setRoomTypes(data);
        } catch (err) {
            console.error('Error fetching room types:', err);
        }
    }, [id]);

    useEffect(() => {
        if (hotel) {
            fetchRoomTypes();
        }
    }, [hotel, fetchRoomTypes]);

    // Image Handlers (from CreateHotel)
    const uploadFile = async (file, isMain = false) => {
        setUploadingImage(true);
        try {
            const formDataUpload = new FormData();
            formDataUpload.append('file', file);
            const data = await api.post('/upload/image', formDataUpload);
            if (isMain) {
                setEditData(prev => ({ ...prev, imageUrl: data.url }));
            }
            return data.url;
        } catch (err) {
            console.error('Upload error:', err);
            setError('Грешка при качване на снимка: ' + err.message);
            return null;
        } finally {
            setUploadingImage(false);
        }
    };

    const handleMainImageFile = async (e) => {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                setEditData(prev => ({ ...prev, imageUrl: reader.result }));
            };
            reader.readAsDataURL(file);
            await uploadFile(file, true);
        }
    };

    const handleImageFile = async (index, e) => {
        const file = e.target.files[0];
        if (file) {
            const newImageFiles = [...imageFiles];
            newImageFiles[index] = file;
            setImageFiles(newImageFiles);

            const reader = new FileReader();
            reader.onloadend = () => {
                const newPreviews = [...imagePreviews];
                newPreviews[index] = reader.result;
                setImagePreviews(newPreviews);
            };
            reader.readAsDataURL(file);
        }
    };

    const addImageField = () => {
        setImages([...images, '']);
        setImageFiles([...imageFiles, null]);
        setImagePreviews([...imagePreviews, null]);
    };

    const removeImageField = (index) => {
        setImages(images.filter((_, i) => i !== index));
        setImageFiles(imageFiles.filter((_, i) => i !== index));
        setImagePreviews(imagePreviews.filter((_, i) => i !== index));
    };

    const updateImage = (index, value) => {
        const newImages = [...images];
        newImages[index] = value;
        setImages(newImages);
    };

    const handleSaveEdit = async () => {
        setError('');
        setSuccess('');
        setUploading(true);

        try {
            // Upload additional images that are files
            const uploadedImageUrls = [];
            for (let i = 0; i < imageFiles.length; i++) {
                if (imageFiles[i]) {
                    const url = await uploadFile(imageFiles[i]);
                    if (url) uploadedImageUrls.push(url);
                } else if (images[i] && images[i].trim()) {
                    uploadedImageUrls.push(images[i]);
                }
            }

            await api.put(`/hotels/${id}`, {
                ...hotel,
                ...editData,
                images: JSON.stringify(uploadedImageUrls),
                cancellationPolicies: JSON.stringify(editData.cancellationPolicies)
            });

            setSuccess(t('savedSuccessfully'));
            setIsEditing(false);
            fetchHotel();
        } catch (err) {
            setError(err.message || t('errorSaving'));
        } finally {
            setUploading(false);
        }
    };

    const handleRoomsChange = (rooms) => {
        setRoomTypes(rooms);
    };

    const fetchModerators = React.useCallback(async () => {
        try {
            const data = await api.get(`/hotels/${id}/moderators`);
            setModerators(data);
        } catch (err) {
            console.error('Error fetching moderators:', err);
        }
    }, [id]);

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
    }, [activeTab, fetchModerators]);

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
                                        {/* Map Location - New */}
                                        <div className="form-section full-width">
                                            <div className="form-section-title">📍 {t('mapLocation') || 'Локация на картата'}</div>
                                            <React.Suspense fallback={<div className="loading-state"><div className="spinner"></div></div>}>
                                                <LocationPicker
                                                    onLocationSelect={(locationData) => {
                                                        setEditData({
                                                            ...editData,
                                                            latitude: locationData.lat,
                                                            longitude: locationData.lng,
                                                            city: locationData.city || editData.city,
                                                            country: locationData.country || editData.country,
                                                            location: locationData.address || editData.location
                                                        });
                                                    }}
                                                    initialLat={editData.latitude}
                                                    initialLng={editData.longitude}
                                                />
                                            </React.Suspense>
                                            <div className="form-row">
                                                <div className="form-group">
                                                    <label>{t('latitude') || 'Географска ширина'}</label>
                                                    <input
                                                        type="number"
                                                        value={editData.latitude || ''}
                                                        onChange={(e) => setEditData({ ...editData, latitude: e.target.value ? parseFloat(e.target.value) : null })}
                                                        step="0.000001"
                                                    />
                                                </div>
                                                <div className="form-group">
                                                    <label>{t('longitude') || 'Географска дължина'}</label>
                                                    <input
                                                        type="number"
                                                        value={editData.longitude || ''}
                                                        onChange={(e) => setEditData({ ...editData, longitude: e.target.value ? parseFloat(e.target.value) : null })}
                                                        step="0.000001"
                                                    />
                                                </div>
                                            </div>
                                        </div>

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

                                        {/* Status and Rating Row */}
                                        <div className="form-group">
                                            <label>{t('status') || 'Статус'}</label>
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
                                            <div className="star-rating-select">
                                                {[0, 1, 2, 3, 4, 5].map((star) => (
                                                    <button
                                                        key={star}
                                                        type="button"
                                                        className={`star-select-btn ${star <= (hoveredStar !== null ? hoveredStar : editData.starRating) ? 'selected' : ''
                                                            } ${star === 0 ? 'zero-star' : ''}`}
                                                        onClick={() => setEditData({ ...editData, starRating: star })}
                                                        onMouseEnter={() => setHoveredStar(star)}
                                                        onMouseLeave={() => setHoveredStar(null)}
                                                    >
                                                        {star === 0 ? '—' : '★'}
                                                    </button>
                                                ))}
                                            </div>
                                        </div>

                                        <div className="form-group full-width">
                                            <label>{t('description')}</label>
                                            <textarea
                                                value={editData.description}
                                                onChange={(e) => setEditData({ ...editData, description: e.target.value })}
                                                rows="4"
                                            />
                                        </div>

                                        {/* Photos Section - New */}
                                        <div className="form-section full-width">
                                            <div className="form-section-title">🖼️ {t('photos') || 'Снимки'}</div>

                                            {/* Main Photo */}
                                            <div className="form-group main-photo-upload">
                                                <label>{t('mainPhoto') || 'Основна снимка'} *</label>
                                                <div className="photo-input-group">
                                                    <input
                                                        type="file"
                                                        accept="image/*"
                                                        onChange={handleMainImageFile}
                                                        style={{ display: 'none' }}
                                                        id="edit-main-image"
                                                    />
                                                    <label htmlFor="edit-main-image" className="btn-upload">
                                                        📁 {t('changePhoto') || 'Смени снимката'}
                                                    </label>
                                                    <input
                                                        type="url"
                                                        value={editData.imageUrl?.startsWith('data:') ? '' : editData.imageUrl}
                                                        onChange={(e) => setEditData({ ...editData, imageUrl: e.target.value })}
                                                        placeholder={t('enterURL') || 'Или въведи URL адрес'}
                                                    />
                                                </div>
                                                {editData.imageUrl && (
                                                    <div className="image-preview-large">
                                                        <img
                                                            src={editData.imageUrl}
                                                            alt="Main Preview"
                                                            onError={(e) => e.target.parentElement.style.display = 'none'}
                                                            onLoad={(e) => e.target.parentElement.style.display = 'block'}
                                                        />
                                                    </div>
                                                )}
                                            </div>

                                            {/* Additional Photos */}
                                            <div className="form-group">
                                                <label>{t('additionalPhotos') || 'Допълнителни снимки'}</label>
                                                <div className="additional-photos-grid">
                                                    {images.map((img, index) => (
                                                        <div key={index} className="photo-card">
                                                            {(imagePreviews[index] || img) ? (
                                                                <img
                                                                    src={imagePreviews[index] || img}
                                                                    alt={`Preview ${index}`}
                                                                    className="preview-thumb"
                                                                    onError={(e) => e.target.style.display = 'none'}
                                                                    onLoad={(e) => e.target.style.display = 'block'}
                                                                />
                                                            ) : (
                                                                <div className="preview-thumb" style={{ background: '#eee', display: 'flex', alignItems: 'center', justifyItems: 'center', justifyContent: 'center' }}>🖼️</div>
                                                            )}
                                                            <input
                                                                type="url"
                                                                className="url-input"
                                                                value={img || ''}
                                                                onChange={(e) => updateImage(index, e.target.value)}
                                                                placeholder="URL"
                                                            />
                                                            <input
                                                                type="file"
                                                                accept="image/*"
                                                                onChange={(e) => handleImageFile(index, e)}
                                                                style={{ display: 'none' }}
                                                                id={`edit-image-${index}`}
                                                            />
                                                            <div style={{ display: 'flex', gap: '5px', marginTop: '5px' }}>
                                                                <label htmlFor={`edit-image-${index}`} className="btn-upload" style={{ padding: '2px 5px', fontSize: '0.8rem', flex: 1 }}>📁</label>
                                                                <button
                                                                    type="button"
                                                                    className="btn-remove-photo"
                                                                    onClick={() => removeImageField(index)}
                                                                >
                                                                    ✕
                                                                </button>
                                                            </div>
                                                        </div>
                                                    ))}
                                                    <button type="button" className="btn-add-photo" onClick={addImageField}>
                                                        <span>➕</span>
                                                        <span style={{ fontSize: '0.8rem' }}>{t('addPhoto') || 'Добави'}</span>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>

                                        <div className="form-group full-width">
                                            <label>{t('cancellationPolicies') || 'Политики за отмяна'}</label>
                                            <div className="dynamic-list">
                                                {editData.cancellationPolicies.map((policy, index) => (
                                                    <div key={index} className="policy-card">
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
                                                            style={{ width: '80px' }}
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
                                                            style={{ width: '80px' }}
                                                        />
                                                        <span>% възстановяване</span>
                                                        <button
                                                            type="button"
                                                            className="btn-remove"
                                                            onClick={() => {
                                                                const newPolicies = editData.cancellationPolicies.filter((_, i) => i !== index);
                                                                setEditData({ ...editData, cancellationPolicies: newPolicies });
                                                            }}
                                                            style={{ marginLeft: 'auto', background: 'none', border: 'none', cursor: 'pointer', color: '#ff4444', fontSize: '1.2rem' }}
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
                                            >
                                                ➕ {t('addCancellationPolicy') || 'Добави правило'}
                                            </button>
                                            <p className="hint-text">
                                                Пример: 60 дни преди настаняване -&gt; 100% възстановяване на сумата. За липсващи дни до самата дата на настаняване (0 дни) се приема 0% (без възстановяване). Ако нямате въведени правила, приемаме, че отмяната е винаги 100% безплатна.
                                            </p>
                                        </div>
                                    </div>
                                    <div className="edit-actions">
                                        <button className="btn-cancel" onClick={() => setIsEditing(false)} disabled={uploading}>
                                            ❌ {t('cancel')}
                                        </button>
                                        <button className="btn-save" onClick={handleSaveEdit} disabled={uploading || uploadingImage}>
                                            {uploading ? '⏳...' : `✅ ${t('save')}`}
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
