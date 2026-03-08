import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';

import './HotelMessages.css';

const HotelMessages = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user } = useAuth();
    const { t } = useLanguage();

    const [messages, setMessages] = useState([]);
    const [hotel, setHotel] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // Reply state
    const [replyingTo, setReplyingTo] = useState(null);
    const [replyText, setReplyText] = useState('');
    const [sendingReply, setSendingReply] = useState(false);

    useEffect(() => {
        if (!user) {
            navigate('/login');
            return;
        }
        fetchData();
    }, [user, navigate, fetchData]);

    const fetchData = React.useCallback(async () => {
        try {
            // Fetch hotel details to verify ownership and get name
            const hotelData = await api.get(`/hotels/${id}`);

            // Check if user is owner, moderator or super admin
            const isOwner = hotelData.createdById === user?.id;
            const isModerator = hotelData.isModerator; // This comes from getHotel endpoint which checks current user
            const isSuperAdmin = user?.roles?.includes('SuperAdmin');

            if (!isOwner && !isModerator && !isSuperAdmin) {
                navigate('/my-hotels');
                return;
            }
            setHotel(hotelData);

            // Fetch messages
            const messagesData = await api.get(`/hotels/${id}/messages`);
            setMessages(messagesData);
        } catch (err) {
            setError(t('errorLoadingData') || 'Error loading data');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [id, user, navigate, t]);

    const handleReplyClick = (messageId) => {
        setReplyingTo(messageId);
        setReplyText('');
    };

    const handleCancelReply = () => {
        setReplyingTo(null);
        setReplyText('');
    };

    const handleSendReply = async (messageId) => {
        if (!replyText.trim()) return;

        setSendingReply(true);
        try {
            await api.put(`/hotels/${id}/messages/${messageId}/answer`, { adminResponse: replyText });

            // Update local state
            setMessages(messages.map(msg =>
                msg.id === messageId
                    ? { ...msg, isAnswered: true, adminResponse: replyText, answeredAt: new Date().toISOString() }
                    : msg
            ));

            setReplyingTo(null);
            setReplyText('');
        } catch {
            alert(t('errorSendingReply') || 'Error sending reply');
        } finally {
            setSendingReply(false);
        }
    };

    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleString();
    };

    if (loading) {
        return (
            <div className="messages-page">

                <div className="loading-container">
                    <div className="spinner"></div>
                </div>
            </div>
        );
    }

    if (!hotel) return null;

    return (
        <div className="messages-page">

            <div className="messages-container">
                <div className="messages-header">
                    <div className="header-left">
                        <Link to="/my-hotels" className="btn-back">← {t('back') || 'Back'}</Link>
                        <h1>{t('messagesFor') || 'Messages -'} {hotel.name}</h1>
                    </div>
                </div>

                {error && <div className="error-message">{error}</div>}

                {messages.length === 0 ? (
                    <div className="empty-state">
                        <h2>{t('noMessages') || 'No messages yet'}</h2>
                        <p>{t('noMessagesDesc') || 'When users send questions to your hotel, they will appear here.'}</p>
                    </div>
                ) : (
                    <div className="messages-list">
                        {messages.map(msg => (
                            <div key={msg.id} className={`message-card ${msg.isAnswered ? 'answered' : 'unanswered'}`}>
                                <div className="message-header-row">
                                    <div className="user-info">
                                        <span className="user-name">{msg.userName}</span>
                                        <span className="user-email">{msg.userEmail}</span>
                                    </div>
                                    <div className="message-date">{formatDate(msg.createdAt)}</div>
                                </div>

                                <h3 className="message-subject">{msg.subject}</h3>
                                <p className="message-body">{msg.message}</p>

                                <div className="message-status">
                                    {msg.isAnswered ? (
                                        <div className="answer-section">
                                            <div className="answer-header">
                                                <span className="status-badge answered">✓ {t('answered') || 'Answered'}</span>
                                                <span className="answer-date">{formatDate(msg.answeredAt)}</span>
                                            </div>
                                            <div className="admin-response">
                                                <strong>{t('yourResponse') || 'Your Response'}:</strong>
                                                <p>{msg.adminResponse}</p>
                                            </div>
                                        </div>
                                    ) : (
                                        <div className="action-section">
                                            <span className="status-badge pending">⏳ {t('pending') || 'Pending'}</span>
                                            {replyingTo === msg.id ? (
                                                <div className="reply-form">
                                                    <textarea
                                                        value={replyText}
                                                        onChange={(e) => setReplyText(e.target.value)}
                                                        placeholder={t('typeReply') || 'Type your reply here...'}
                                                        rows="4"
                                                    />
                                                    <div className="reply-actions">
                                                        <button
                                                            className="btn-cancel"
                                                            onClick={handleCancelReply}
                                                            disabled={sendingReply}
                                                        >
                                                            {t('cancel') || 'Cancel'}
                                                        </button>
                                                        <button
                                                            className="btn-reply-send"
                                                            onClick={() => handleSendReply(msg.id)}
                                                            disabled={sendingReply}
                                                        >
                                                            {sendingReply ? (t('sending') || 'Sending...') : (t('sendReply') || 'Send Reply')}
                                                        </button>
                                                    </div>
                                                </div>
                                            ) : (
                                                <button
                                                    className="btn-reply"
                                                    onClick={() => handleReplyClick(msg.id)}
                                                >
                                                    ↩️ {t('reply') || 'Reply'}
                                                </button>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

export default HotelMessages;
