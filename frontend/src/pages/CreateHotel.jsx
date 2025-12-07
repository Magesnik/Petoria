import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Header from '../components/Header';
import './CreateHotel.css';

const CreateHotel = () => {
    const { isAdmin } = useAuth();
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        location: '',
        city: '',
        country: '',
        pricePerNight: '',
        rating: '',
        imageUrl: '',
        images: '',
        amenities: '',
        roomTypes: ''
    });
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    // Redirect if not admin
    React.useEffect(() => {
        if (!isAdmin()) {
            navigate('/');
        }
    }, [isAdmin, navigate]);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess(false);

        try {
            const token = localStorage.getItem('token');

            // Parse JSON fields safely
            let images = [];
            let amenities = [];
            let roomTypes = [];

            try {
                if (formData.images && formData.images.trim()) {
                    images = JSON.parse(formData.images);
                }
            } catch (e) {
                throw new Error('Невалиден формат на снимките. Използвайте JSON масив.');
            }

            try {
                if (formData.amenities && formData.amenities.trim()) {
                    amenities = JSON.parse(formData.amenities);
                }
            } catch (e) {
                throw new Error('Невалиден формат на удобствата. Използвайте JSON масив.');
            }

            try {
                if (formData.roomTypes && formData.roomTypes.trim()) {
                    roomTypes = JSON.parse(formData.roomTypes);
                }
            } catch (e) {
                throw new Error('Невалиден формат на типовете стаи. Използвайте JSON масив.');
            }

            // Build hotel data - convert JSON arrays to strings for backend
            const hotelData = {
                name: formData.name,
                description: formData.description,
                location: formData.location,
                city: formData.city,
                country: formData.country,
                pricePerNight: parseFloat(formData.pricePerNight),
                rating: parseFloat(formData.rating),
                imageUrl: formData.imageUrl || '',
                images: JSON.stringify(images),
                amenities: JSON.stringify(amenities),
                roomTypes: JSON.stringify(roomTypes)
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
                    // Handle validation errors
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
                imageUrl: '',
                images: '',
                amenities: '',
                roomTypes: ''
            });

            // Redirect to hotels page after 2 seconds
            setTimeout(() => {
                navigate('/hotels');
            }, 2000);
        } catch (err) {
            setError(err.message || 'Failed to create hotel. Please check your input.');
            console.error(err);
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

                    <form onSubmit={handleSubmit}>
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
                            />
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label>Локация *</label>
                                <input
                                    type="text"
                                    name="location"
                                    value={formData.location}
                                    onChange={handleChange}
                                    required
                                    maxLength="300"
                                    placeholder="Адрес"
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
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label>URL на основна снимка</label>
                            <input
                                type="url"
                                name="imageUrl"
                                value={formData.imageUrl}
                                onChange={handleChange}
                                maxLength="500"
                                placeholder="https://example.com/image.jpg"
                            />
                        </div>

                        <div className="form-group">
                            <label>Допълнителни снимки (JSON масив)</label>
                            <input
                                type="text"
                                name="images"
                                value={formData.images}
                                onChange={handleChange}
                                placeholder='["url1", "url2"]'
                            />
                            <small>Формат: JSON масив от URL-и</small>
                        </div>

                        <div className="form-group">
                            <label>Удобства (JSON масив)</label>
                            <input
                                type="text"
                                name="amenities"
                                value={formData.amenities}
                                onChange={handleChange}
                                placeholder='["WiFi", "Басейн", "Паркинг"]'
                            />
                            <small>Формат: JSON масив от удобства</small>
                        </div>

                        <div className="form-group">
                            <label>Типове стаи (JSON масив)</label>
                            <input
                                type="text"
                                name="roomTypes"
                                value={formData.roomTypes}
                                onChange={handleChange}
                                placeholder='["Единична", "Двойна", "Апартамент"]'
                            />
                            <small>Формат: JSON масив от типове стаи</small>
                        </div>

                        <button type="submit" className="btn btn-primary btn-block">
                            Създай хотел
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default CreateHotel;
