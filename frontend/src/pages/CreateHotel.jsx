import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
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
        pricePerNight: '',
        rating: '',
        imageUrl: ''
    });

    const [images, setImages] = useState([]);
    const [imageFiles, setImageFiles] = useState([null]);
    const [imagePreviews, setImagePreviews] = useState([null]);
    const [amenities, setAmenities] = useState([]);
    const [customAmenity, setCustomAmenity] = useState('');
    const [roomTypes, setRoomTypes] = useState(['']);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);
    const [uploading, setUploading] = useState(false);

    // Популярни удобства
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

    // Room type management
    const addRoomType = () => {
        setRoomTypes([...roomTypes, '']);
    };

    const removeRoomType = (index) => {
        setRoomTypes(roomTypes.filter((_, i) => i !== index));
    };

    const updateRoomType = (index, value) => {
        const newRoomTypes = [...roomTypes];
        newRoomTypes[index] = value;
        setRoomTypes(newRoomTypes);
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

            // Filter out empty room types
            const filteredRoomTypes = roomTypes.filter(rt => rt.trim() !== '');

            // Build hotel data
            const hotelData = {
                name: formData.name,
                description: formData.description,
                location: formData.location,
                city: formData.city,
                country: formData.country,
                pricePerNight: parseFloat(formData.pricePerNight),
                rating: parseFloat(formData.rating),
                imageUrl: formData.imageUrl || '',
                images: JSON.stringify(uploadedImageUrls),
                amenities: JSON.stringify(amenities),
                roomTypes: JSON.stringify(filteredRoomTypes)
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
                pricePerNight: '',
                rating: '',
                imageUrl: ''
            });
            setImages([]);
            setImageFiles([null]);
            setImagePreviews([null]);
            setAmenities([]);
            setRoomTypes(['']);

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
                    <h2>Създай нов хотел</h2>
                    <p className="subtitle">Попълнете информацията за новия хотел</p>

                    {error && <div className="error-message">{error}</div>}
                    {success && <div className="success-message">Хотелът беше създаден успешно!</div>}
                    {uploading && <div className="uploading-message">📤 Качване на снимки...</div>}

                    <form onSubmit={handleSubmit}>
                        {/* Basic Info */}
                        <div className="form-section">
                            <h3 className="section-title">📋 Основна информация</h3>

                            <div className="form-row">
                                <div className="form-group">
                                    <label>Име на хотела *</label>
                                    <input
                                        type="text"
                                        name="name"
                                        value={formData.name}
                                        onChange={handleChange}
                                        required
                                        maxLength="200"
                                        placeholder="Hotel Paradise"
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Град *</label>
                                    <input
                                        type="text"
                                        name="city"
                                        value={formData.city}
                                        onChange={handleChange}
                                        required
                                        maxLength="100"
                                        placeholder="София"
                                    />
                                </div>
                            </div>

                            <div className="form-group">
                                <label>Описание</label>
                                <textarea
                                    name="description"
                                    value={formData.description}
                                    onChange={handleChange}
                                    rows="4"
                                    maxLength="2000"
                                    placeholder="Опишете хотела, неговите особености и предимства..."
                                />
                                <small>{formData.description.length}/2000 символа</small>
                            </div>

                            <div className="form-row">
                                <div className="form-group">
                                    <label>Локация (Адрес) *</label>
                                    <input
                                        type="text"
                                        name="location"
                                        value={formData.location}
                                        onChange={handleChange}
                                        required
                                        maxLength="300"
                                        placeholder="ул. Витоша 123"
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Държава *</label>
                                    <input
                                        type="text"
                                        name="country"
                                        value={formData.country}
                                        onChange={handleChange}
                                        required
                                        maxLength="100"
                                        placeholder="България"
                                    />
                                </div>
                            </div>

                            <div className="form-row">
                                <div className="form-group">
                                    <label>Цена на нощувка (лв) *</label>
                                    <input
                                        type="number"
                                        name="pricePerNight"
                                        value={formData.pricePerNight}
                                        onChange={handleChange}
                                        required
                                        min="0"
                                        step="0.01"
                                        placeholder="150.00"
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Рейтинг (0-5) *</label>
                                    <input
                                        type="number"
                                        name="rating"
                                        value={formData.rating}
                                        onChange={handleChange}
                                        required
                                        min="0"
                                        max="5"
                                        step="0.1"
                                        placeholder="4.5"
                                    />
                                </div>
                            </div>
                        </div>

                        {/* Images */}
                        <div className="form-section">
                            <h3 className="section-title">🖼️ Снимки</h3>

                            <div className="form-group">
                                <label>Основна снимка *</label>
                                <div className="file-input-wrapper">
                                    <input
                                        type="file"
                                        accept="image/*"
                                        onChange={handleMainImageFile}
                                        className="file-input"
                                        id="main-image"
                                    />
                                    <label htmlFor="main-image" className="file-label">
                                        📁 Избери файл
                                    </label>
                                    <span className="file-hint">или </span>
                                    <input
                                        type="url"
                                        name="imageUrl"
                                        value={formData.imageUrl.startsWith('data:') ? '' : formData.imageUrl}
                                        onChange={handleChange}
                                        placeholder="въведи URL"
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
                                <label>Допълнителни снимки</label>
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
                                                    placeholder={`URL на снимка ${index + 1}`}
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
                                    + Добави още снимка
                                </button>
                            </div>
                        </div>

                        {/* Amenities */}
                        <div className="form-section">
                            <h3 className="section-title">⭐ Удобства</h3>
                            <p className="section-description">Изберете удобствата, които предлага хотелът</p>

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
                                    placeholder="Добави свое удобство..."
                                    onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addCustomAmenity())}
                                />
                                <button type="button" onClick={addCustomAmenity} className="btn-add-amenity">
                                    + Добави
                                </button>
                            </div>

                            {amenities.length > 0 && (
                                <div className="selected-amenities">
                                    <h4>Избрани удобства:</h4>
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
                                        Общо: {amenities.length} удобства
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Room Types */}
                        <div className="form-section">
                            <h3 className="section-title">🛏️ Типове стаи</h3>

                            <div className="dynamic-list">
                                {roomTypes.map((roomType, index) => (
                                    <div key={index} className="dynamic-item">
                                        <input
                                            type="text"
                                            value={roomType}
                                            onChange={(e) => updateRoomType(index, e.target.value)}
                                            placeholder={`Тип стая ${index + 1} (напр. Единична, Двойна, Апартамент)`}
                                        />
                                        {roomTypes.length > 1 && (
                                            <button
                                                type="button"
                                                className="btn-remove"
                                                onClick={() => removeRoomType(index)}
                                            >
                                                ✕
                                            </button>
                                        )}
                                    </div>
                                ))}
                            </div>
                            <button type="button" className="btn-add" onClick={addRoomType}>
                                + Добави още тип стая
                            </button>
                        </div>

                        <button type="submit" className="btn btn-primary btn-block" disabled={uploading}>
                            {uploading ? '⏳ Качване...' : '✨ Създай хотел'}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default CreateHotel;
