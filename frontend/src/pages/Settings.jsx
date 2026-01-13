import React, { useState } from 'react';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import Header from '../components/Header';
import './Settings.css';

const Settings = () => {
    const { t } = useLanguage();
    const { user } = useAuth();
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

    const handleProfileSubmit = (e) => {
        e.preventDefault();
        // TODO: Add API call to update profile
        setMessage(t('profileUpdated'));
        setTimeout(() => setMessage(''), 3000);
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
            <Header />
            <div className="settings-page">
                <div className="settings-hero">
                    <h1>{t('settingsTitle')}</h1>
                    <p>{t('settingsSubtitle')}</p>
                </div>

                <div className="settings-container">
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
