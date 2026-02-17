import React, { useState, useRef } from 'react';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { useCurrency } from '../context/CurrencyContext';

import './Settings.css';

const Settings = () => {
    const { t, language, toggleLanguage } = useLanguage();
    const { theme, toggleTheme } = useTheme();
    const { currency, changeCurrency, availableCurrencies } = useCurrency();
    const { user, login } = useAuth();
    const [formData, setFormData] = useState({
        firstName: user?.firstName || '',
        lastName: user?.lastName || '',
        email: user?.email || '',
    });
    const [passwordData, setPasswordData] = useState({
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
    });
    const [message, setMessage] = useState('');
    const [passwordMessage, setPasswordMessage] = useState('');
    const [avatarMessage, setAvatarMessage] = useState('');
    const [avatarPreview, setAvatarPreview] = useState(user?.avatarUrl || null);
    const [uploading, setUploading] = useState(false);
    const fileInputRef = useRef(null);

    const handleProfileChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    };

    const handlePasswordChange = (e) => {
        setPasswordData({
            ...passwordData,
            [e.target.name]: e.target.value,
        });
    };

    const handleProfileSubmit = async (e) => {
        e.preventDefault();
        try {
            const data = await api.put('/profile', {
                firstName: formData.firstName,
                lastName: formData.lastName
            });

            // Update user in context
            login({ ...user, firstName: data.user.firstName, lastName: data.user.lastName });
            setMessage(t('profileUpdated') || 'Profile updated successfully');
            setTimeout(() => setMessage(''), 3000);
        } catch (error) {
            console.error('Error updating profile:', error);
            setMessage('Error updating profile');
        }
    };

    const handleAvatarChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            // Validate file type
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png'];
            if (!allowedTypes.includes(file.type)) {
                setAvatarMessage('Only JPG, JPEG, and PNG files are allowed');
                setTimeout(() => setAvatarMessage(''), 3000);
                return;
            }

            // Validate file size (max 2MB)
            if (file.size > 2 * 1024 * 1024) {
                setAvatarMessage('File size must be less than 2MB');
                setTimeout(() => setAvatarMessage(''), 3000);
                return;
            }

            // Show preview
            const reader = new FileReader();
            reader.onloadend = () => {
                setAvatarPreview(reader.result);
            };
            reader.readAsDataURL(file);

            // Upload immediately
            handleAvatarUpload(file);
        }
    };

    const handleAvatarUpload = async (file) => {
        setUploading(true);
        try {
            const formData = new FormData();
            formData.append('file', file);

            const data = await api.post('/profile/avatar', formData);

            const fullAvatarUrl = `http://localhost:5150${data.avatarUrl}`;
            setAvatarPreview(fullAvatarUrl);
            // Update user in context
            login({ ...user, avatarUrl: fullAvatarUrl });
            setAvatarMessage('Avatar uploaded successfully');
            setTimeout(() => setAvatarMessage(''), 3000);
        } catch (error) {
            console.error('Error uploading avatar:', error);
            setAvatarMessage('Error uploading avatar');
        } finally {
            setUploading(false);
        }
    };

    const handleAvatarRemove = async () => {
        try {
            await api.delete('/profile/avatar');

            setAvatarPreview(null);
            // Update user in context
            login({ ...user, avatarUrl: null });
            setAvatarMessage('Avatar removed successfully');
            setTimeout(() => setAvatarMessage(''), 3000);
        } catch (error) {
            console.error('Error removing avatar:', error);
            setAvatarMessage('Error removing avatar');
        }
    };

    const handlePasswordSubmit = (e) => {
        e.preventDefault();
        if (passwordData.newPassword !== passwordData.confirmPassword) {
            setPasswordMessage('Passwords do not match!');
            setTimeout(() => setPasswordMessage(''), 3000);
            return;
        }
        // TODO: Add API call to update password
        setPasswordMessage(t('passwordUpdated'));
        setPasswordData({
            currentPassword: '',
            newPassword: '',
            confirmPassword: '',
        });
        setTimeout(() => setPasswordMessage(''), 3000);
    };

    return (
        <>

            <div className="settings-page">
                <div className="settings-hero">
                    <h1>{t('settingsTitle')}</h1>
                    <p>{t('settingsSubtitle')}</p>
                </div>

                <div className="settings-container">
                    {/* Avatar Section */}
                    <div className="settings-section avatar-section">
                        <h2>{t('profilePicture') || 'Profile Picture'}</h2>
                        <div className="avatar-upload-container">
                            <div className="avatar-preview">
                                {avatarPreview ? (
                                    <img src={avatarPreview} alt="Avatar" />
                                ) : (
                                    <div className="avatar-placeholder">👤</div>
                                )}
                            </div>
                            <div className="avatar-actions">
                                <input
                                    ref={fileInputRef}
                                    type="file"
                                    accept="image/jpeg,image/jpg,image/png"
                                    onChange={handleAvatarChange}
                                    style={{ display: 'none' }}
                                />
                                <button
                                    type="button"
                                    className="btn btn-primary"
                                    onClick={() => fileInputRef.current?.click()}
                                    disabled={uploading}
                                >
                                    {uploading ? 'Uploading...' : (t('uploadPicture') || 'Upload Picture')}
                                </button>
                                {avatarPreview && (
                                    <button
                                        type="button"
                                        className="btn btn-secondary"
                                        onClick={handleAvatarRemove}
                                    >
                                        {t('removePicture') || 'Remove Picture'}
                                    </button>
                                )}
                            </div>
                            {avatarMessage && <div className="info-message">{avatarMessage}</div>}
                        </div>
                    </div>

                    {/* Preferences Section */}
                    <div className="settings-section">
                        <h2>{t('preferences') || 'Preferences'}</h2>

                        <div className="preferences-grid">
                            {/* Language Toggle */}
                            <div className="preference-item">
                                <div className="preference-info">
                                    <span className="preference-icon">🌐</span>
                                    <div>
                                        <h3>{t('language') || 'Language'}</h3>
                                        <p className="preference-description">
                                            {language === 'en' ? 'English' : 'Български'}
                                        </p>
                                    </div>
                                </div>
                                <button
                                    onClick={toggleLanguage}
                                    className="btn-toggle"
                                >
                                    {language === 'en' ? '🇧🇬 БГ' : '🇬🇧 EN'}
                                </button>
                            </div>

                            {/* Theme Toggle */}
                            <div className="preference-item">
                                <div className="preference-info">
                                    <span className="preference-icon">{theme === 'dark' ? '🌙' : '☀️'}</span>
                                    <div>
                                        <h3>{t('theme') || 'Theme'}</h3>
                                        <p className="preference-description">
                                            {theme === 'dark'
                                                ? (t('darkMode') || 'Dark Mode')
                                                : (t('lightMode') || 'Light Mode')
                                            }
                                        </p>
                                    </div>
                                </div>
                                <button
                                    onClick={toggleTheme}
                                    className="btn-toggle"
                                >
                                    {theme === 'dark' ? '☀️' : '🌙'}
                                </button>
                            </div>

                            {/* Currency Selector */}
                            <div className="preference-item">
                                <div className="preference-info">
                                    <span className="preference-icon">💱</span>
                                    <div>
                                        <h3>{t('currency') || 'Currency'}</h3>
                                        <p className="preference-description">
                                            {t('selectCurrency') || 'Select your preferred currency'}
                                        </p>
                                    </div>
                                </div>
                                <select
                                    value={currency}
                                    onChange={(e) => changeCurrency(e.target.value)}
                                    className="currency-select"
                                >
                                    {availableCurrencies.map(curr => (
                                        <option key={curr} value={curr}>
                                            {curr}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        </div>
                    </div>

                    {/* Profile Information Section */}
                    <div className="settings-section">
                        <h2>{t('profileInfo')}</h2>
                        <form onSubmit={handleProfileSubmit} className="settings-form">
                            <div className="form-group">
                                <label htmlFor="firstName">{t('firstNameLabel')}</label>
                                <input
                                    type="text"
                                    id="firstName"
                                    name="firstName"
                                    value={formData.firstName}
                                    onChange={handleProfileChange}
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="lastName">{t('lastNameLabel')}</label>
                                <input
                                    type="text"
                                    id="lastName"
                                    name="lastName"
                                    value={formData.lastName}
                                    onChange={handleProfileChange}
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="email">{t('emailLabel')}</label>
                                <input
                                    type="email"
                                    id="email"
                                    name="email"
                                    value={formData.email}
                                    onChange={handleProfileChange}
                                    required
                                />
                            </div>
                            {message && <div className="success-message">{message}</div>}
                            <button type="submit" className="btn btn-primary">
                                {t('saveChanges')}
                            </button>
                        </form>
                    </div>

                    {/* Change Password Section */}
                    <div className="settings-section">
                        <h2>{t('changePassword')}</h2>
                        <form onSubmit={handlePasswordSubmit} className="settings-form">
                            <div className="form-group">
                                <label htmlFor="currentPassword">{t('currentPassword')}</label>
                                <input
                                    type="password"
                                    id="currentPassword"
                                    name="currentPassword"
                                    value={passwordData.currentPassword}
                                    onChange={handlePasswordChange}
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="newPassword">{t('newPassword')}</label>
                                <input
                                    type="password"
                                    id="newPassword"
                                    name="newPassword"
                                    value={passwordData.newPassword}
                                    onChange={handlePasswordChange}
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="confirmPassword">{t('confirmPassword')}</label>
                                <input
                                    type="password"
                                    id="confirmPassword"
                                    name="confirmPassword"
                                    value={passwordData.confirmPassword}
                                    onChange={handlePasswordChange}
                                    required
                                />
                            </div>
                            {passwordMessage && (
                                <div className={passwordMessage.includes('success') || passwordMessage.includes('успешно') ? 'success-message' : 'error-message'}>
                                    {passwordMessage}
                                </div>
                            )}
                            <button type="submit" className="btn btn-primary">
                                {t('saveChanges')}
                            </button>
                        </form>
                    </div>
                </div>
            </div>
        </>
    );
};

export default Settings;
