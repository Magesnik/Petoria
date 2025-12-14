import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { GoogleOAuthProvider, GoogleLogin } from '@react-oauth/google';
import { jwtDecode } from 'jwt-decode';
import { useLanguage } from '../context/LanguageContext';
import { useAuth } from '../context/AuthContext';
import Header from '../components/Header';
import './Auth.css';

const GOOGLE_CLIENT_ID = "21847094498-c94136osjkahal0fjg0nk9q4mc7e4um9.apps.googleusercontent.com";

const Login = () => {
    const { t } = useLanguage();
    const { login } = useAuth();
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
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
            const response = await fetch('http://localhost:5150/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || 'Login failed');
            }

            // Store token
            login({
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                roles: data.roles || []
            }, data.token);

            navigate('/');
        } catch (err) {
            setError('Invalid email or password');
            console.error(err);
        }
    };

    const handleGoogleSuccess = async (credentialResponse) => {
        try {
            const decoded = jwtDecode(credentialResponse.credential);

            const response = await fetch('http://localhost:5150/api/auth/google-login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    idToken: credentialResponse.credential,
                    email: decoded.email,
                    firstName: decoded.given_name,
                    lastName: decoded.family_name,
                    externalUserId: decoded.sub
                })
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || 'Google login failed');
            }

            login({
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                roles: data.roles || []
            }, data.token);

            navigate('/');
        } catch (err) {
            setError('Google login failed. Please try again.');
            console.error(err);
        }
    };

    const handleGoogleError = () => {
        setError('Google login failed. Please try again.');
    };

    return (
        <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
            <div className="auth-page">
                <Header />
                <div className="auth-container">
                    <div className="auth-card">
                        <h2>{t('loginTitle')}</h2>
                        <p className="auth-subtitle">{t('loginSubtitle')}</p>

                        {error && <div className="error-message">{error}</div>}

                        <div className="social-login">
                            <GoogleLogin
                                onSuccess={handleGoogleSuccess}
                                onError={handleGoogleError}
                                useOneTap
                                size="large"
                                theme="outline"
                                text="signin_with"
                            />
                        </div>

                        <div className="divider">
                            <span>ИЛИ</span>
                        </div>

                        <form onSubmit={handleSubmit}>
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
                                />
                            </div>

                            <button type="submit" className="btn btn-primary btn-block">
                                {t('loginBtn')}
                            </button>
                        </form>

                        <div className="auth-footer">
                            {t('noAccount')} <Link to="/register">{t('registerLink')}</Link>
                        </div>
                    </div>
                </div>
            </div>
        </GoogleOAuthProvider>
    );
};

export default Login;
