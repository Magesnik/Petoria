import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';

import './ContactHotel.css';

/** Страница за изпращане на съобщение до хотела от потребителя. */
const ContactHotel = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user } = useAuth();
    const { t } = useLanguage();

    const [hotel, setHotel] = useState(null);
    // Данни от формуляра за контакт
    const [formData, setFormData] = useState({
        subject: '',
        message: ''
    });
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const fetchHotel = React.useCallback(async () => {
        try {
            const data = await api.get(`/hotels/${id}`);
            setHotel(data);
        } catch (err) {
            setError(t('errorLoadingHotel') || 'Error loading hotel');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [id, t]);

    useEffect(() => {
        if (!user) {
            navigate('/login');
            return;
        }
        fetchHotel();
    }, [user, navigate, fetchHotel]);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSubmitting(true);

        try {
            await api.post(`/hotels/${id}/messages`, formData);

            setSuccess(true);
            setTimeout(() => {
                navigate(`/hotel/${id}`);
            }, 3000);
        } catch (err) {
            if (err.message?.includes("10 unanswered messages")) {
                setError(t('messageLimitReachedError') || 'You have reached the maximum limit of 10 unanswered messages.');
            } else {
                setError(err.message);
            }
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return (
            <div className="contact-page">

                <div className="loading-container">
                    <div className="spinner"></div>
                </div>
            </div>
        );
    }

    if (!hotel) {
        return null;
    }

    return (
        <div className="contact-page">

            <div className="contact-container">
                <Link to={`/hotel/${id}`} className="btn-back">
                    ← {t('backToHotel') || 'Back to Hotel'}
                </Link>

                <div className="contact-card">
                    <div className="contact-header">
                        <h1>{t('contactSupport') || 'Contact Support'}</h1>
                        <p className="hotel-name">{hotel.name}</p>
                        <p className="subtitle">{t('askQuestion') || 'Have a question? Send a message to the hotel administration.'}</p>
                    </div>

                    {success ? (
                        <div className="success-state">
                            <div className="success-icon">✅</div>
                            <h2>{t('messageSent') || 'Message Sent!'}</h2>
                            <p>{t('messageSentDesc') || 'Your message has been sent to the hotel administration. You will be redirected shortly.'}</p>
                            <Link to={`/hotel/${id}`} className="btn-primary">
                                {t('backToHotel') || 'Back to Hotel'}
                            </Link>
                        </div>
                    ) : (
                        <form onSubmit={handleSubmit} className="contact-form">
                            {error && <div className="error-message">{error}</div>}

                            <div className="form-group">
                                <label htmlFor="subject">{t('subject') || 'Subject'} *</label>
                                <input
                                    type="text"
                                    id="subject"
                                    name="subject"
                                    value={formData.subject}
                                    onChange={handleChange}
                                    required
                                    maxLength="200"
                                    placeholder={t('subjectPlaceholder') || 'e.g. Question about parking'}
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="message">{t('message') || 'Message'} *</label>
                                <textarea
                                    id="message"
                                    name="message"
                                    value={formData.message}
                                    onChange={handleChange}
                                    required
                                    minLength="10"
                                    maxLength="2000"
                                    rows="6"
                                    placeholder={t('messagePlaceholder') || 'Type your message here...'}
                                />
                                <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '5px' }}>
                                    <small className="text-muted">{t('messageMinLengthInfo') || 'Message must be at least 10 characters long.'}</small>
                                    <small className="char-count">{formData.message.length}/2000</small>
                                </div>
                            </div>

                            <button
                                type="submit"
                                className="btn-submit"
                                disabled={submitting}
                            >
                                {submitting ? (t('sending') || 'Sending...') : (t('sendMessage') || 'Send Message')}
                            </button>
                        </form>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ContactHotel;
