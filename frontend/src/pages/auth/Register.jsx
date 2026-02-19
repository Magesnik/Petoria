import React, { useState } from 'react';
import { api } from '../../utils/api';
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

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        try {
            const data = await api.post('/auth/register', formData);

            // Store token with avatar URL
            const avatarUrl = data.avatarUrl && data.avatarUrl.startsWith('/uploads/')
                ? `http://localhost:5150${data.avatarUrl}`
                : data.avatarUrl;

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
            setError(err.message || 'Registration failed');
            console.error(err);
        }
    };

    const handleGoogleSuccess = async (credentialResponse) => {
        try {
            const data = await api.post('/auth/google-login', { googleToken: credentialResponse.credential });

            // Store token with avatar URL
            const avatarUrl = data.avatarUrl && data.avatarUrl.startsWith('/uploads/')
                ? `http://localhost:5150${data.avatarUrl}`
                : data.avatarUrl;

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
                </div>
            </div>
        </div>
    );
};

export default Register;
