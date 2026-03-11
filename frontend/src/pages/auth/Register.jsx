import React, { useState } from 'react';
import { api, getAssetUrl } from '../../utils/api';
import { Link, useNavigate } from 'react-router-dom';
import { GoogleLogin } from '@react-oauth/google';
import { useLanguage } from '../../context/LanguageContext';
import { useAuth } from '../../context/AuthContext';

import './Auth.css';

const Register = () => {
    const { t } = useLanguage();
    const { login } = useAuth();
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        email: '',
        password: ''
    });
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const validateForm = () => {
        const nameRegex = /^[a-zA-Zа-яА-Я]+$/;
        const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;

        if (formData.firstName.length < 2 || !nameRegex.test(formData.firstName)) {
            setError(t('firstNameInvalid'));
            return false;
        }

        if (formData.lastName.length < 4 || !nameRegex.test(formData.lastName)) {
            setError(t('lastNameInvalid'));
            return false;
        }

        // Basic email regex
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(formData.email)) {
            // Can use a generic error or rely on HTML5 type="email"
            setError(t('emailInvalid'));
            return false;
        }

        if (formData.password.length < 6 || !passwordRegex.test(formData.password)) {
            setError(t('passwordInvalid'));
            return false;
        }

        return true;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (!validateForm()) {
            return;
        }

        try {
            await api.post('/auth/register', formData);
            setSuccess(true);
        } catch (err) {
            setError(err.message || 'Registration failed');
            console.error(err);
        }
    };

    const handleGoogleSuccess = async (credentialResponse) => {
        try {
            const data = await api.post('/auth/google-login', { googleToken: credentialResponse.credential });

            // Token is stored in HttpOnly cookie by the server
            const avatarUrl = getAssetUrl(data.avatarUrl);

            login({
                id: data.id,
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                roles: data.roles || [],
                avatarUrl: avatarUrl
            });

            navigate('/');
        } catch (err) {
            setError(err.message || 'Google authentication failed. Please try again.');
            console.error(err);
        }
    };

    const handleGoogleError = () => {
        setError('Google authentication failed. Please try again.');
    };

    return (
        <div className="auth-page">

            <div className="auth-container">
                <div className="auth-card">
                    <h2>{t('registerTitle')}</h2>
                    <p className="auth-subtitle">{t('registerSubtitle')}</p>

                    {error && <div className="error-message">{error}</div>}

                    {success ? (
                        <div className="success-message" style={{ textAlign: 'center', padding: '20px' }}>
                            <div style={{ fontSize: '48px', color: '#10b981', marginBottom: '16px' }}>✉️</div>
                            <h3>{t('registrationSuccessTitle')}</h3>
                            <p>{t('registrationSuccessMessage')}</p>
                            <Link to="/login" className="btn btn-primary" style={{ marginTop: '20px', display: 'inline-block' }}>
                                {t('loginBtn')}
                            </Link>
                        </div>
                    ) : (
                        <>
                            <form onSubmit={handleSubmit}>
                                <div className="form-row">
                                    <div className="form-group">
                                        <label>{t('firstNameLabel')}</label>
                                        <input
                                            type="text"
                                            name="firstName"
                                            value={formData.firstName}
                                            onChange={handleChange}
                                            required
                                            minLength="2"
                                            pattern="^[a-zA-Zа-яА-Я]+$"
                                            title={t('firstNameInvalid')}
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>{t('lastNameLabel')}</label>
                                        <input
                                            type="text"
                                            name="lastName"
                                            value={formData.lastName}
                                            onChange={handleChange}
                                            required
                                            minLength="4"
                                            pattern="^[a-zA-Zа-яА-Я]+$"
                                            title={t('lastNameInvalid')}
                                        />
                                    </div>
                                </div>

                                <div className="form-group">
                                    <label>{t('emailLabel')}</label>
                                    <input
                                        type="email"
                                        name="email"
                                        value={formData.email}
                                        onChange={handleChange}
                                        required
                                    />
                                </div>

                                <div className="form-group">
                                    <label>{t('passwordLabel')}</label>
                                    <input
                                        type="password"
                                        name="password"
                                        value={formData.password}
                                        onChange={handleChange}
                                        required
                                        minLength="6"
                                        pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$"
                                        title={t('passwordInvalid')}
                                    />
                                </div>

                                <button type="submit" className="btn btn-primary btn-block">
                                    {t('registerBtn')}
                                </button>
                            </form>

                            <div className="auth-divider">
                                <span>ИЛИ</span>
                            </div>

                            <div className="google-login-wrapper">
                                <GoogleLogin
                                    onSuccess={handleGoogleSuccess}
                                    onError={handleGoogleError}
                                    theme="outline"
                                    size="large"
                                    width="100%"
                                />
                            </div>

                            <div className="auth-footer">
                                {t('hasAccount')} <Link to="/login">{t('loginLink')}</Link>
                            </div>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Register;
