import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';

import './AdminSupportMessages.css';

const AdminSupportMessages = () => {
    const { isSuperAdmin } = useAuth();
    const { t } = useLanguage();
    const navigate = useNavigate();

    const [messages, setMessages] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [replyText, setReplyText] = useState('');
    const [replyingTo, setReplyingTo] = useState(null);
    const [sendingReply, setSendingReply] = useState(false);

    useEffect(() => {
        if (!isSuperAdmin()) {
            navigate('/');
            return;
        }
        fetchMessages();
    }, [isSuperAdmin, navigate]);

    const fetchMessages = async () => {
        try {
            const data = await api.get('/support/messages/admin');
            setMessages(data);
        } catch (err) {
            setError(t('errorLoadingMessages') || 'Error loading messages');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleReplySubmit = async (messageId) => {
        if (!replyText.trim()) return;

        setSendingReply(true);
        try {
            await api.put(`/support/messages/${messageId}/answer`, { response: replyText });

            // Refresh messages
            fetchMessages();
            setReplyingTo(null);
            setReplyText('');
        } catch (err) {
            console.error('Error sending reply:', err);
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
            <div className="admin-support-page">

                <div className="loading-container">
                    <div className="spinner"></div>
                </div>
            </div>
        );
    }

    return (
        <div className="admin-support-page">

            <div className="admin-container">
                <div className="page-header">
                    <button className="btn-back" onClick={() => navigate('/admin')}>
                        ← {t('backToDashboard')}
                    </button>
                    <h1>{t('supportMessagesTitle')}</h1>
                </div>

                {error && <div className="error-message">{error}</div>}

                <div className="messages-list">
                    {messages.length === 0 ? (
                        <div className="empty-state">
                            <p>{t('noSupportMessages')}</p>
                        </div>
                    ) : (
                        messages.map(msg => (
                            <div key={msg.id} className={`support-message-card ${msg.isAnswered ? 'answered' : 'pending'}`}>
                                <div className="message-header">
                                    <div className="user-info">
                                        <span className="user-name">{msg.userName}</span>
                                        <span className="user-email">{msg.userEmail}</span>
                                    </div>
                                    <div className="message-meta">
                                        <span className={`status-badge ${msg.isAnswered ? 'answered' : 'pending'}`}>
                                            {msg.isAnswered ? t('answered') : t('pending')}
                                        </span>
                                        <span className="message-date">{formatDate(msg.createdAt)}</span>
                                    </div>
                                </div>

                                <div className="message-content">
                                    <h3>{msg.subject}</h3>
                                    <p>{msg.message}</p>
                                </div>

                                {msg.isAnswered ? (
                                    <div className="admin-response">
                                        <strong>{t('yourResponse')}:</strong>
                                        <p>{msg.adminResponse}</p>
                                        <span className="response-date">{formatDate(msg.answeredAt)}</span>
                                    </div>
                                ) : (
                                    <div className="reply-section">
                                        {replyingTo === msg.id ? (
                                            <div className="reply-form">
                                                <textarea
                                                    value={replyText}
                                                    onChange={(e) => setReplyText(e.target.value)}
                                                    placeholder={t('writeReplyPlaceholder')}
                                                    rows="4"
                                                />
                                                <div className="reply-actions">
                                                    <button
                                                        className="btn-submit"
                                                        onClick={() => handleReplySubmit(msg.id)}
                                                        disabled={sendingReply}
                                                    >
                                                        {sendingReply ? t('sending') : t('sendReply')}
                                                    </button>
                                                    <button
                                                        className="btn-cancel"
                                                        onClick={() => {
                                                            setReplyingTo(null);
                                                            setReplyText('');
                                                        }}
                                                    >
                                                        {t('cancel')}
                                                    </button>
                                                </div>
                                            </div>
                                        ) : (
                                            <button
                                                className="btn-reply"
                                                onClick={() => setReplyingTo(msg.id)}
                                            >
                                                ↩ {t('reply')}
                                            </button>
                                        )}
                                    </div>
                                )}
                            </div>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
};

export default AdminSupportMessages;
