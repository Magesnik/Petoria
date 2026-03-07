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
    const [filter, setFilter] = useState('all');
    const [deletingId, setDeletingId] = useState(null);

    const filteredMessages = messages.filter(msg => {
        if (filter === 'answered') return msg.isAnswered;
        if (filter === 'pending') return !msg.isAnswered;
        return true;
    });

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

    const handleDeleteMessage = async (messageId) => {
        if (!window.confirm(t('confirmDeleteMessage'))) return;
        
        setDeletingId(messageId);
        try {
            await api.delete(`/support/messages/${messageId}`);
            setMessages(prev => prev.filter(m => m.id !== messageId));
        } catch (err) {
            console.error('Error deleting message:', err);
            alert(t('errorDeletingMessage') || 'Error deleting message');
        } finally {
            setDeletingId(null);
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

                <div className="filter-controls">
                    <button 
                        className={`btn-filter ${filter === 'all' ? 'active' : ''}`}
                        onClick={() => setFilter('all')}
                    >
                        {t('filterAll')}
                    </button>
                    <button 
                        className={`btn-filter ${filter === 'pending' ? 'active' : ''}`}
                        onClick={() => setFilter('pending')}
                    >
                        {t('filterPending')}
                    </button>
                    <button 
                        className={`btn-filter ${filter === 'answered' ? 'active' : ''}`}
                        onClick={() => setFilter('answered')}
                    >
                        {t('filterAnswered')}
                    </button>
                </div>

                <div className="messages-list">
                    {filteredMessages.length === 0 ? (
                        <div className="empty-state">
                            <p>{t('noSupportMessages')}</p>
                        </div>
                    ) : (
                        filteredMessages.map(msg => (
                            <div key={msg.id} className={`support-message-card ${msg.isAnswered ? 'answered' : 'pending'}`}>
                                <div className="message-header">
                                    <div className="user-info">
                                        <span className="user-name">{msg.userName}</span>
                                        <span className="user-email">{msg.userEmail}</span>
                                    </div>
                                    <div className="message-meta-container">
                                        <div className="message-meta">
                                            <span className={`status-badge ${msg.isAnswered ? 'answered' : 'pending'}`}>
                                                {msg.isAnswered ? t('answered') : t('pending')}
                                            </span>
                                            <span className="message-date">{formatDate(msg.createdAt)}</span>
                                        </div>
                                        <button 
                                            className="btn-delete" 
                                            onClick={() => handleDeleteMessage(msg.id)}
                                            disabled={deletingId === msg.id}
                                            title={t('deleteMessage')}
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                                <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/>
                                                <path fillRule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>
                                            </svg>
                                            <span>{t('deleteMessage')}</span>
                                        </button>
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
