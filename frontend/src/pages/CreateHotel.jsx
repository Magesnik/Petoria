import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import LocationPicker from '../components/LocationPicker';
import './CreateHotel.css';

const CreateHotel = () => {
    const { isAdmin } = useAuth();
    const { t } = useLanguage();
    const navigate = useNavigate();

    const [formData, setFormData] = useState({
        name: '',
        description: '',
        location: '',
        city: '',
        country: '',
        latitude: null,
        longitude: null,
        starRating: 3,
        imageUrl: ''
    });

    const [images, setImages] = useState([]);
    const [imageFiles, setImageFiles] = useState([null]);
    const [imagePreviews, setImagePreviews] = useState([null]);
    const [amenities, setAmenities] = useState([]);
    const [customAmenity, setCustomAmenity] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);
    const [uploading, setUploading] = useState(false);
    const [hoveredStar, setHoveredStar] = useState(null);

    // Популярни удобства - keeping Bulgarian since these are specific amenity names
    const popularAmenities = [
        'WiFi', 'Басейн', 'Паркинг', 'Фитнес', 'Ресторант',
        'Спа', 'Климатик', 'Рум сървиз', 'Бар', 'Конферентна зала',
        'Трансфер', 'Градина', 'Тераса', 'Сауна', 'Детска площадка'
    ];

    // Redirect if not admin
    React.useEffect(() => {
        if (!isAdmin()) {
            navigate('/');
        }
    }, [isAdmin, navigate]);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    // Main image file upload
    const handleMainImageFile = async (e) => {
        const file = e.target.files[0];
        if (file) {
            // Show preview
            const reader = new FileReader();
            reader.onloadend = () => {
                setFormData({ ...formData, imageUrl: reader.result });
            };
            reader.readAsDataURL(file);

            // Upload file
            await uploadFile(file, true);
        }
    };

    // Additional images file upload
    const handleImageFile = async (index, e) => {
        const file = e.target.files[0];
        if (file) {
            const newImageFiles = [...imageFiles];
            newImageFiles[index] = file;
            setImageFiles(newImageFiles);

            // Show preview
            const reader = new FileReader();
            reader.onloadend = () => {
                const newPreviews = [...imagePreviews];
                newPreviews[index] = reader.result;
                setImagePreviews(newPreviews);
            };
            reader.readAsDataURL(file);
        }
    };

    // Upload file to server
    const uploadFile = async (file, isMain = false) => {
        setUploading(true);
        try {
            const formDataUpload = new FormData();
            formDataUpload.append('file', file);

            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5150/api/upload/image', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                },
                body: formDataUpload
            });

            if (!response.ok) {
                throw new Error('Failed to upload image');
            }

            const data = await response.json();

            if (isMain) {
                setFormData({ ...formData, imageUrl: data.url });
            }

            return data.url;
        } catch (err) {
            console.error('Upload error:', err);
            setError('Грешка при качване на снимка: ' + err.message);
            return null;
        } finally {
            setUploading(false);
        }
    };

    // Add image field
    const addImageField = () => {
        setImages([...images, '']);
        setImageFiles([...imageFiles, null]);
        setImagePreviews([...imagePreviews, null]);
    };

    // Remove image field
    const removeImageField = (index) => {
        setImages(images.filter((_, i) => i !== index));
        setImageFiles(imageFiles.filter((_, i) => i !== index));
        setImagePreviews(imagePreviews.filter((_, i) => i !== index));
    };

    // Update image URL
    const updateImage = (index, value) => {
        const newImages = [...images];
        newImages[index] = value;
        setImages(newImages);
    };

    // Amenity management
    const toggleAmenity = (amenity) => {
        if (amenities.includes(amenity)) {
            setAmenities(amenities.filter(a => a !== amenity));
        } else {
            setAmenities([...amenities, amenity]);
        }
    };

    // Add custom amenity
    const addCustomAmenity = () => {
        if (customAmenity.trim() && !amenities.includes(customAmenity.trim())) {
            setAmenities([...amenities, customAmenity.trim()]);
            setCustomAmenity('');
        }
    };

    // Remove amenity
    const removeAmenity = (amenity) => {
        setAmenities(amenities.filter(a => a !== amenity));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess(false);
        setUploading(true);

        try {
            const token = localStorage.getItem('token');

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

            // Build hotel data
            const hotelData = {
                name: formData.name,
                description: formData.description,
                location: formData.location,
                city: formData.city,
                country: formData.country,
                latitude: formData.latitude,
                longitude: formData.longitude,
                pricePerNight: 0, // Will be set via room types later
                rating: 0, // Initial user rating
                starRating: parseInt(formData.starRating),
                imageUrl: formData.imageUrl || '',
                images: JSON.stringify(uploadedImageUrls),
                amenities: JSON.stringify(amenities),
                roomTypes: '[]' // Empty for now, will be managed separately
            };

            const response = await fetch('http://localhost:5150/api/hotels', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(hotelData)
            });

            if (!response.ok) {
                let errorMessage = 'Failed to create hotel';
                try {
                    const data = await response.json();
                    if (data.errors) {
                        const errorMessages = Object.entries(data.errors)
                            .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
                            .join('\n');
                        errorMessage = errorMessages;
                    } else {
                        errorMessage = data.title || data.message || JSON.stringify(data);
                    }
                } catch (e) {
                    errorMessage = `HTTP ${response.status}: ${response.statusText}`;
                }
                throw new Error(errorMessage);
            }

            setSuccess(true);
            // Reset form
            setFormData({
                name: '',
                description: '',
                location: '',
                city: '',
                country: '',
                latitude: null,
                longitude: null,
                starRating: 3,
                imageUrl: ''
            });
            setImages([]);
            setImageFiles([null]);
            setImagePreviews([null]);
            setAmenities([]);

            // Redirect to hotels page after 2 seconds
            setTimeout(() => {
                navigate('/hotels');
            }, 2000);
        } catch (err) {
            setError(err.message || 'Failed to create hotel. Please check your input.');
            console.error(err);
        } finally {
            setUploading(false);
        }
    };

    if (!isAdmin()) {
        return null;
    }

    return (
        <div className="create-hotel-page">
            <Header />
            <div className="create-hotel-container">
                <div className="create-hotel-card">
                    <h2>{t('createNewHotel')}</h2>
                    <p className="subtitle">{t('fillHotelInfo')}</p>

                    {error && <div className="error-message">{error}</div>}
                    {success && <div className="success-message">{t('hotelCreatedSuccess')}</div>}
                    {uploading && <div className="uploading-message">📤 {t('uploading')}</div>}

                    <form onSubmit={handleSubmit}>
                        {/* Location Picker - FIRST */}
                        <div className="form-section">
                            <h3 className="section-title">📍 {t('mapLocation')}</h3>
                            <p className="section-description">{t('selectLocationDescription')}</p>

                            <LocationPicker
                                onLocationSelect={(locationData) => {
                                    setFormData({
                                        ...formData,
                                        latitude: locationData.lat,
                                        longitude: locationData.lng,
                                        city: locationData.city || formData.city,
                                        country: locationData.country || formData.country,
                                        location: locationData.address || formData.location
                                    });
                                }}
                                initialLat={formData.latitude}
                                initialLng={formData.longitude}
                            />

                            <div className="form-row" style={{ marginTop: '15px' }}>
                                <div className="form-group">
                                    <label>{t('latitude')}</label>
                                    <input
                                        type="number"
                                        value={formData.latitude || ''}
                                        onChange={(e) => setFormData({ ...formData, latitude: e.target.value ? parseFloat(e.target.value) : null })}
                                        step="0.000001"
                                        placeholder="42.697708"
                                        className="coordinate-input"
                                    />
                                </div>
                                <div className="form-group">
                                    <label>{t('longitude')}</label>
                                    <input
                                        type="number"
                                        value={formData.longitude || ''}
                                        onChange={(e) => setFormData({ ...formData, longitude: e.target.value ? parseFloat(e.target.value) : null })}
                                        step="0.000001"
                                        placeholder="23.321868"
                                        className="coordinate-input"
                                    />
                                </div>
                            </div>
                        </div>

                        {/* Basic Info */}
                        <div className="form-section">
                            <h3 className="section-title">📋 {t('basicInformation')}</h3>

                            <div className="form-row">
                                <div className="form-group">
                                    <label>{t('hotelName')} *</label>
                                    <input
                                        type="text"
                                        name="name"
                                        value={formData.name}
                                        onChange={handleChange}
                                        required
                                        maxLength="200"
                                        placeholder={t('hotelNamePlaceholder')}
                                    />
                                </div>
                                <div className="form-group">
                                    <label>{t('city')} *</label>
                                    <input
                                        type="text"
                                        name="city"
                                        value={formData.city}
                                        onChange={handleChange}
                                        required
                                        maxLength="100"
                                        placeholder={t('cityPlaceholder')}
                                    />
                                </div>
                            </div>

                            <div className="form-group">
                                <label>{t('descriptionLabel')}</label>
                                <textarea
                                    name="description"
                                    value={formData.description}
                                    onChange={handleChange}
                                    rows="4"
                                    maxLength="2000"
                                    placeholder={t('descriptionPlaceholder')}
                                />
                                <small>{formData.description.length}/2000 {t('charactersCount')}</small>
                            </div>

                            <div className="form-row">
                                <div className="form-group">
                                    <label>{t('locationAddress')} *</label>
                                    <input
                                        type="text"
                                        name="location"
                                        value={formData.location}
                                        onChange={handleChange}
                                        required
                                        maxLength="300"
                                        placeholder={t('addressPlaceholder')}
                                    />
                                </div>
                                <div className="form-group">
                                    <label>{t('country')} *</label>
                                    <input
                                        type="text"
                                        name="country"
                                        value={formData.country}
                                        onChange={handleChange}
                                        required
                                        maxLength="100"
                                        placeholder={t('countryPlaceholder')}
                                    />
                                </div>
                            </div>

                            <div className="form-group">
                                <label>{t('starRating')} *</label>
                                <div className="star-rating-select">
                                    {[0, 1, 2, 3, 4, 5].map((star) => (
                                        <button
                                            key={star}
                                            type="button"
                                            className={`star-select-btn ${
                                                star <= (hoveredStar !== null ? hoveredStar : formData.starRating) ? 'selected' : ''
                                            } ${star === 0 ? 'zero-star' : ''}`}
                                            onClick={() => setFormData({ ...formData, starRating: star })}
                                            onMouseEnter={() => setHoveredStar(star)}
                                            onMouseLeave={() => setHoveredStar(null)}
                                            title={star === 0 ? t('noCategory') : `${star} ${star === 1 ? t('star') : t('starsCount')}`}
                                        >
                                            {star === 0 ? '—' : '★'}
                                        </button>
                                    ))}
                                </div>
                                <small className="rating-hint">
                                    {formData.starRating === 0 ? t('noCategory') : `${formData.starRating} ${formData.starRating === 1 ? t('star') : t('starsCount')}`}
                                </small>
                                <input
                                    type="hidden"
                                    name="starRating"
                                    value={formData.starRating}
                                    required
                                />
                            </div>
                        </div>

                        {/* Images */}
                        <div className="form-section">
                            <h3 className="section-title">🖼️ {t('photos')}</h3>

                            <div className="form-group">
                                <label>{t('mainPhoto')} *</label>
                                <div className="file-input-wrapper">
                                    <input
                                        type="file"
                                        accept="image/*"
                                        onChange={handleMainImageFile}
                                        className="file-input"
                                        id="main-image"
                                    />
                                    <label htmlFor="main-image" className="file-label">
                                        📁 {t('chooseFile')}
                                    </label>
                                    <span className="file-hint">{t('orText')} </span>
                                    <input
                                        type="url"
                                        name="imageUrl"
                                        value={formData.imageUrl.startsWith('data:') ? '' : formData.imageUrl}
                                        onChange={handleChange}
                                        placeholder={t('enterURL')}
                                        className="url-input"
                                    />
                                </div>
                                {formData.imageUrl && (
                                    <div className="image-preview">
                                        <img
                                            src={formData.imageUrl}
                                            alt="Preview"
                                            onError={(e) => e.target.style.display = 'none'}
                                        />
                                    </div>
                                )}
                            </div>

                            <div className="form-group">
                                <label>{t('additionalPhotos')}</label>
                                <div className="dynamic-list">
                                    {imageFiles.map((file, index) => (
                                        <div key={index} className="dynamic-item">
                                            <div className="file-input-wrapper compact">
                                                <input
                                                    type="file"
                                                    accept="image/*"
                                                    onChange={(e) => handleImageFile(index, e)}
                                                    className="file-input"
                                                    id={`image-${index}`}
                                                />
                                                <label htmlFor={`image-${index}`} className="file-label compact">
                                                    📁
                                                </label>
                                                <input
                                                    type="url"
                                                    value={images[index] || ''}
                                                    onChange={(e) => updateImage(index, e.target.value)}
                                                    placeholder={`${t('photoURL')} ${index + 1}`}
                                                    className="url-input"
                                                />
                                            </div>
                                            {imagePreviews[index] && (
                                                <div className="image-preview-small">
                                                    <img src={imagePreviews[index]} alt={`Preview ${index}`} />
                                                </div>
                                            )}
                                            {imageFiles.length > 1 && (
                                                <button
                                                    type="button"
                                                    className="btn-remove"
                                                    onClick={() => removeImageField(index)}
                                                >
                                                    ✕
                                                </button>
                                            )}
                                        </div>
                                    ))}
                                </div>
                                <button type="button" className="btn-add" onClick={addImageField}>
                                    {t('addMorePhoto')}
                                </button>
                            </div>
                        </div>

                        {/* Amenities */}
                        <div className="form-section">
                            <h3 className="section-title">⭐ {t('amenities')}</h3>
                            <p className="section-description">{t('selectAmenities')}</p>

                            <div className="amenities-grid">
                                {popularAmenities.map((amenity) => (
                                    <button
                                        key={amenity}
                                        type="button"
                                        className={`amenity-tag ${amenities.includes(amenity) ? 'selected' : ''}`}
                                        onClick={() => toggleAmenity(amenity)}
                                    >
                                        {amenities.includes(amenity) && '✓ '}
                                        {amenity}
                                    </button>
                                ))}
                            </div>

                            {/* Custom amenity input */}
                            <div className="custom-amenity-input">
                                <input
                                    type="text"
                                    value={customAmenity}
                                    onChange={(e) => setCustomAmenity(e.target.value)}
                                    placeholder={t('addCustomAmenity')}
                                    onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addCustomAmenity())}
                                />
                                <button type="button" onClick={addCustomAmenity} className="btn-add-amenity">
                                    {t('addButton')}
                                </button>
                            </div>

                            {amenities.length > 0 && (
                                <div className="selected-amenities">
                                    <h4>{t('selectedAmenities')}</h4>
                                    <div className="selected-tags">
                                        {amenities.map((amenity, index) => (
                                            <span key={index} className="selected-tag">
                                                {amenity}
                                                <button
                                                    type="button"
                                                    onClick={() => removeAmenity(amenity)}
                                                    className="remove-tag"
                                                >
                                                    ✕
                                                </button>
                                            </span>
                                        ))}
                                    </div>
                                    <div className="selected-count">
                                        {t('totalAmenities').replace('{count}', amenities.length)}
                                    </div>
                                </div>
                            )}
                        </div>



                        <button type="submit" className="btn btn-primary btn-block" disabled={uploading}>
                            {uploading ? `⏳ ${t('uploading')}` : `✨ ${t('createHotelButton')}`}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default CreateHotel;
