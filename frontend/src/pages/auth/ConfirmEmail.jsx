import React, { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { api } from '../../utils/api';
import { useLanguage } from '../../context/LanguageContext';
import './ConfirmEmail.css'; // We'll add some basic styles next or inline

const ConfirmEmail = () => {
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState('confirming'); // 'confirming', 'success', 'error'
    const { t } = useLanguage();

    useEffect(() => {
        const confirmEmail = async () => {
            const uid = searchParams.get('uid');
            const token = searchParams.get('token');

            if (!uid || !token) {
                setStatus('error');
                return;
            }

            try {
                // Ensure token uses proper URL encoding if it hasn't been parsed correctly
                await api.post('/auth/confirm-email', {
                    userId: uid,
                    token: token
                });
                setStatus('success');
            } catch (error) {
                console.error('Email confirmation error:', error);
                setStatus('error');
            }
        };

        confirmEmail();
    }, [searchParams]);

    return (
        <div className="auth-page">
            <div className="auth-container">
                <div className="auth-card" style={{ textAlign: 'center' }}>
                    <h2>{t('emailConfirmationTitle')}</h2>

                    {status === 'confirming' && (
                        <div>
                            <p>{t('confirmingEmail')}</p>
                            <div className="loading-spinner" style={{ margin: '20px auto' }}></div>
                        </div>
                    )}

                    {status === 'success' && (
                        <div>
                            <div className="success-icon" style={{ fontSize: '48px', color: '#10b981', marginBottom: '16px' }}>✓</div>
                            <p>{t('emailConfirmSuccess')}</p>
                            <Link to="/login" className="btn btn-primary" style={{ display: 'inline-block', marginTop: '20px' }}>
                                {t('loginBtn')}
                            </Link>
                        </div>
                    )}

                    {status === 'error' && (
                        <div>
                            <div className="error-icon" style={{ fontSize: '48px', color: '#ef4444', marginBottom: '16px' }}>✕</div>
                            <p>{t('emailConfirmError')}</p>
                            <Link to="/register" className="btn" style={{ display: 'inline-block', marginTop: '20px' }}>
                                {t('registerBtn')}
                            </Link>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ConfirmEmail;
