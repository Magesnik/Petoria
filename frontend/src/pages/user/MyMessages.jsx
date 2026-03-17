import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';

import './MyMessages.css';

/** Страница с всички съобщения на потребителя — хотелски и поддръжка, с филтри. */
const MyMessages = () => {
    const { user } = useAuth();
    const { t, language } = useLanguage();
    const navigate = useNavigate();

    const [messages, setMessages] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    // Активен филтър: 'all' | 'answered' | 'pending'
    const [filter, setFilter] = useState('all');

    const markSupportMessagesAsRead = React.useCallback(async (messages) => {
        for (const msg of messages) {
            try {
                await api.put(`/support/messages/${msg.id}/read`);
            } catch (err) {
                console.error('Error marking support message as read:', err);
            }
        }
    }, []);

    const markMessagesAsRead = React.useCallback(async (unreadMessages) => {
        // We mark them as read in the background without blocking UI
        for (const msg of unreadMessages) {
            try {
                await api.put(`/hotels/my/messages/${msg.id}/read`);
            } catch (err) {
                console.error('Error marking message as read:', err);
            }
        }
    }, []);

    const fetchMessages = React.useCallback(async () => {
        try {
            let hotelMessages = [];
            let supportMessages = [];

            try {
                hotelMessages = await api.get('/hotels/my/messages');
            } catch (e) {
                console.error("Failed to fetch hotel messages", e);
                // If hotel messages fail, we probably should show error as in original code
                throw e;
            }

            try {
                supportMessages = await api.get('/support/messages/my');
            } catch (e) {
                console.warn("Failed to fetch support messages", e);
                // Support messages optional or failed, array remains empty
            }

            let combinedMessages = [];

            // Process Hotel Messages
            const processedHotelMessages = hotelMessages.map(msg => ({
                ...msg,
                type: 'hotel',
                displayTo: msg.hotelName,
                linkTo: `/hotel/${msg.hotelId}`
            }));
            combinedMessages = [...processedHotelMessages];

            // Process Support Messages
            if (supportMessages.length > 0) {
                const processedSupportMessages = supportMessages.map(msg => ({
                    ...msg,
                    type: 'support',
                    displayTo: t('superAdmin') || 'Super Admin',
                    linkTo: '/support'
                }));
                combinedMessages = [...combinedMessages, ...processedSupportMessages];
            }

            // Sort by date descending
            combinedMessages.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
            setMessages(combinedMessages);

            // Mark answered messages as read (Hotel messages only for now as per previous logic)
            const unreadAnsweredMessages = processedHotelMessages.filter(m => m.isAnswered && !m.isReadByUser);
            if (unreadAnsweredMessages.length > 0) {
                markMessagesAsRead(unreadAnsweredMessages);
            }

            // Mark support messages as read
            const unreadSupport = supportMessages.filter(m => m.isAnswered && !m.isReadByUser);
            if (unreadSupport.length > 0) {
                markSupportMessagesAsRead(unreadSupport);
            }

        } catch (err) {
            setError(t('errorLoadingData') || 'Error loading messages');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [t, markMessagesAsRead, markSupportMessagesAsRead]);

    useEffect(() => {
        if (!user) {
            navigate('/login');
            return;
        }
        fetchMessages();
    }, [user, navigate, fetchMessages]);

    const handleReplyClick = (hotelId) => {
        navigate(`/hotel/${hotelId}/contact`);
    };

    const handleDeleteMessage = async (id, type) => {
        if (!window.confirm(t('confirmDeleteMessage') || 'Are you sure you want to delete this message?')) {
            return;
        }

        try {
            if (type === 'hotel') {
                await api.delete(`/hotels/my/messages/${id}`);
            } else if (type === 'support') {
                await api.delete(`/support/messages/my/${id}`);
            }

            // Remove from local state
            setMessages(prev => prev.filter(m => !(m.id === id && m.type === type)));
        } catch (err) {
            console.error('Error deleting message:', err);
            alert(err.message || 'Failed to delete message');
        }
    };

    const formatDate = (dateString) => {
        const locale = language === 'bg' ? 'bg-BG' : 'en-US';
        return new Date(dateString).toLocaleString(locale);
    };

    if (loading) {
        return (
            <div className="my-messages-page">

                <div className="loading-container">
                    <div className="spinner"></div>
                </div>
            </div>
        );
    }

    return (
        <div className="my-messages-page">

            <div className="messages-container">
                <div className="messages-header">
                    <h1>{t('myMessages') || 'My Support Messages'}</h1>
                    <p className="subtitle">{t('myMessagesSubtitle')}</p>
                </div>

                {error && <div className="error-message">{error}</div>}

                {messages.length === 0 ? (
                    <div className="empty-state">
                        <h2>{t('noMessages') || 'No messages yet'}</h2>
                        <p>{t('noMessagesUserDesc') || 'You haven\'t contacted any hotels yet.'}</p>
                        <Link to="/hotels" className="btn-browse">
                            {t('browseHotels') || 'Browse Hotels'}
                        </Link>
                    </div>
                ) : (
                    <>
                        <div className="messages-filters">
                            <button
                                className={`filter-btn ${filter === 'all' ? 'active' : ''}`}
                                onClick={() => setFilter('all')}
                            >
                                {t('filterAll') || t('all') || 'All'}
                            </button>
                            <button
                                className={`filter-btn ${filter === 'pending' ? 'active' : ''}`}
                                onClick={() => setFilter('pending')}
                            >
                                {t('filterPending') || t('pending') || 'Pending'}
                            </button>
                            <button
                                className={`filter-btn ${filter === 'answered' ? 'active' : ''}`}
                                onClick={() => setFilter('answered')}
                            >
                                {t('filterAnswered') || t('answered') || 'Answered'}
                            </button>
                        </div>
                        <div className="messages-list">
                            {messages.filter(msg => {
                                if (filter === 'answered') return msg.isAnswered;
                                if (filter === 'pending') return !msg.isAnswered;
                                return true;
                            }).map(msg => (
                                <div key={`${msg.type}-${msg.id}`} className={`message-card ${msg.isAnswered ? 'answered' : 'pending'}`}>
                                    <div className="message-header-row">
                                        <div className="hotel-info">
                                            <span className="hotel-name-label">{t('to')}:</span>
                                            <Link to={msg.linkTo} className="hotel-link">
                                                {msg.displayTo}
                                            </Link>
                                        </div>
                                        <div className="message-date">{formatDate(msg.createdAt)}</div>
                                    </div>

                                    <h3 className="message-subject">{msg.subject}</h3>
                                    <p className="message-body">{msg.message}</p>

                                    <div className="message-status">
                                        {msg.isAnswered ? (
                                            <div className="answer-section">
                                                <div className="answer-header">
                                                    <span className="status-badge answered">✓ {t('responseReceived') || 'Response Received'}</span>
                                                    <span className="answer-date">{formatDate(msg.answeredAt)}</span>
                                                </div>
                                                <div className="admin-response">
                                                    <strong>{t('hotelResponse') || 'Hotel Response'}:</strong>
                                                    <p>{msg.adminResponse}</p>
                                                </div>

                                                {msg.type === 'hotel' && (
                                                    <button
                                                        className="btn-reply-again"
                                                        onClick={() => handleReplyClick(msg.hotelId)}
                                                    >
                                                        📝 {t('sendAnotherMessage')}
                                                    </button>
                                                )}
                                                {msg.type === 'support' && (
                                                    <button
                                                        className="btn-reply-again"
                                                        onClick={() => navigate('/support')}
                                                    >
                                                        📝 {t('sendAnotherMessage')}
                                                    </button>
                                                )}
                                            </div>
                                        ) : (
                                            <div className="pending-section" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                                <span className="status-badge pending">⏳ {t('waitingForResponse') || 'Waiting for response'}</span>
                                                <button
                                                    className="btn-danger"
                                                    style={{ padding: '6px 12px', fontSize: '0.9rem', borderRadius: '6px', border: 'none', background: '#dc3545', color: 'white', cursor: 'pointer' }}
                                                    onClick={() => handleDeleteMessage(msg.id, msg.type)}
                                                >
                                                    🗑️ {t('delete') || 'Delete'}
                                                </button>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            ))}
                            {messages.filter(msg => {
                                if (filter === 'answered') return msg.isAnswered;
                                if (filter === 'pending') return !msg.isAnswered;
                                return true;
                            }).length === 0 && (
                                    <div className="empty-state" style={{ padding: '2rem', textAlign: 'center' }}>
                                        <p>{t('noMessagesFilter') || 'No messages match the selected filter.'}</p>
                                    </div>
                                )}
                        </div>
                    </>
                )}
            </div>
        </div>
    );
};

export default MyMessages;
