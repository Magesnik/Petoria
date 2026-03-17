import React, { useState, useEffect } from 'react';
import { api, getAssetUrl } from '../../utils/api';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import GlobalPromoCodes from './GlobalPromoCodes';

import './AdminDashboard.css';

/** Административно табло за SuperAdmin — потребители, модератори, хотели и статистики. */
const AdminDashboard = () => {
    const { isSuperAdmin } = useAuth();
    const { t, language } = useLanguage();
    const navigate = useNavigate();
    // Основни данни — потребители, модератори, хотели и статистики
    const [users, setUsers] = useState([]);
    const [moderators, setModerators] = useState([]);
    const [adminHotels, setAdminHotels] = useState([]);
    const [stats, setStats] = useState(null);
    // Избран потребител за преглед на детайли
    const [selectedUser, setSelectedUser] = useState(null);
    const [userDetails, setUserDetails] = useState(null);
    const [loadingUsers, setLoadingUsers] = useState(true);
    const [loadingStats, setLoadingStats] = useState(true);
    const [loadingModerators, setLoadingModerators] = useState(true);
    const [loadingHotels, setLoadingHotels] = useState(true);
    // Термини за търсене по секции
    const [searchTerm, setSearchTerm] = useState('');
    const [searchModeratorTerm, setSearchModeratorTerm] = useState('');
    const [searchHotelTerm, setSearchHotelTerm] = useState('');
    const [error, setError] = useState(null);
    const [unreadMessagesCount, setUnreadMessagesCount] = useState(0);
    // Активен таб и видимост на промо кодовете
    const [activeDashboardTab, setActiveDashboardTab] = useState('users');
    const [showPromoCodes, setShowPromoCodes] = useState(false);

    const getStatusLabel = (status) => {
        switch (status?.toLowerCase()) {
            case 'confirmed': return t('statusConfirmed');
            case 'pending': return t('statusPending');
            case 'cancelled': return t('statusCancelled');
            case 'completed': return t('statusCompleted');
            default: return status;
        }
    };

    const fetchData = () => {
        // Each section loads independently — page renders progressively
        api.get('/admin/users')
            .then(data => setUsers(data))
            .catch(err => setError(err.message))
            .finally(() => setLoadingUsers(false));

        api.get('/admin/stats')
            .then(data => setStats(data))
            .catch(() => {})
            .finally(() => setLoadingStats(false));

        api.get('/support/messages/admin/unread-count')
            .then(count => setUnreadMessagesCount(count))
            .catch(() => {});

        api.get('/admin/moderators')
            .then(data => setModerators(data))
            .catch(() => {})
            .finally(() => setLoadingModerators(false));

        api.get('/admin/hotels')
            .then(data => setAdminHotels(data))
            .catch(() => {})
            .finally(() => setLoadingHotels(false));
    };

    useEffect(() => {
        if (!isSuperAdmin()) {
            navigate('/');
            return;
        }
        fetchData();
    }, [isSuperAdmin, navigate]);

    const fetchUserDetails = async (userId) => {
        try {
            const data = await api.get(`/admin/users/${userId}`);
            setUserDetails(data);
            setSelectedUser(userId);
        } catch (err) {
            setError(err.message);
        }
    };

    const handleDeleteReview = async (hotelId, reviewId) => {
        if (!window.confirm(t('confirmDeleteReview') || 'Наистина ли искате да изтриете този отзив?')) return;
        try {
            await api.delete(`/hotels/${hotelId}/reviews/${reviewId}`);
            if (selectedUser) fetchUserDetails(selectedUser);
            fetchData();
        } catch (err) {
            alert(err.message);
        }
    };

    const handleDeleteComment = async (commentId) => {
        if (!window.confirm(t('confirmDeleteComment') || 'Наистина ли искате да изтриете този коментар?')) return;
        try {
            await api.delete(`/comments/${commentId}`);
            if (selectedUser) fetchUserDetails(selectedUser);
            fetchData();
        } catch (err) {
            alert(err.message);
        }
    };

    const promoteUser = async (userId) => {
        try {
            await api.put(`/admin/users/${userId}/promote`);
            fetchData();
            if (selectedUser === userId) {
                fetchUserDetails(userId);
            }
        } catch (err) {
            setError(err.message);
        }
    };

    const demoteUser = async (userId) => {
        try {
            await api.delete(`/admin/users/${userId}/demote`);
            fetchData();
            if (selectedUser === userId) {
                fetchUserDetails(userId);
            }
        } catch (err) {
            setError(err.message);
        }
    };

    const promoteSuperAdmin = async (userId) => {
        try {
            await api.put(`/admin/users/${userId}/promote-super`);
            fetchData();
            if (selectedUser === userId) fetchUserDetails(userId);
        } catch (err) {
            setError(err.message);
        }
    };

    const demoteSuperAdmin = async (userId) => {
        try {
            await api.delete(`/admin/users/${userId}/demote-super`);
            fetchData();
            if (selectedUser === userId) fetchUserDetails(userId);
        } catch (err) {
            setError(err.message);
        }
    };

    const deleteUser = async (userId) => {
        if (!window.confirm(t('confirmDeleteUser') || 'Наистина ли искате да изтриете този потребител? Това действие е необратимо!')) return;
        try {
            await api.delete(`/admin/users/${userId}`);
            setUsers(prev => prev.filter(u => u.id !== userId));
            setSelectedUser(null);
            setUserDetails(null);
            fetchData();
        } catch (err) {
            setError(err.message);
        }
    };

    const blockUser = async (userId) => {
        if (!window.confirm(t('confirmBlockUser'))) return;
        try {
            await api.put(`/admin/users/${userId}/block`);
            fetchData();
            if (selectedUser === userId) fetchUserDetails(userId);
        } catch (err) {
            setError(err.message);
        }
    };

    const unblockUser = async (userId) => {
        try {
            await api.put(`/admin/users/${userId}/unblock`);
            fetchData();
            if (selectedUser === userId) fetchUserDetails(userId);
        } catch (err) {
            setError(err.message);
        }
    };

    const handleRemoveModeratorRole = async (id) => {
        if (!window.confirm(t('confirmRemoveModeratorRole') || 'Are you sure you want to remove this moderator assignment?')) return;
        try {
            await api.delete(`/admin/moderators/${id}`);
            fetchData();
        } catch (err) {
            setError(err.message);
        }
    };

    const filteredUsers = users.filter(u =>
        u.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        u.firstName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        u.lastName?.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const filteredModerators = moderators.filter(m =>
        m.userFullName?.toLowerCase().includes(searchModeratorTerm.toLowerCase()) ||
        m.userEmail?.toLowerCase().includes(searchModeratorTerm.toLowerCase()) ||
        m.hotelName?.toLowerCase().includes(searchModeratorTerm.toLowerCase())
    );

    const filteredAdminHotels = adminHotels.filter(h =>
        h.name?.toLowerCase().includes(searchHotelTerm.toLowerCase()) ||
        h.city?.toLowerCase().includes(searchHotelTerm.toLowerCase()) ||
        h.ownerEmail?.toLowerCase().includes(searchHotelTerm.toLowerCase())
    );

    const toggleHotelSuspend = async (hotelId) => {
        try {
            const res = await api.post(`/admin/hotels/${hotelId}/toggle-suspend`);
            setAdminHotels(prev => prev.map(h =>
                h.id === hotelId ? { ...h, isSuspendedBySuperAdmin: res.isSuspendedBySuperAdmin } : h
            ));
        } catch (err) {
            setError(err.message);
        }
    };

    if (!isSuperAdmin()) {
        return null;
    }

    return (
        <>

            <div className="admin-dashboard">
                <div className="dashboard-header">
                    <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', justifyContent: 'center' }}>
                        <h1>🛡️ {t('superAdminDashboard')}</h1>
                        <button
                            className="promo-codes-head-btn"
                            onClick={() => {
                                setShowPromoCodes(true);
                                setUserDetails(null);
                            }}
                            title={t('globalPromoCodes')}
                            style={{
                                background: 'none',
                                border: 'none',
                                fontSize: '1.5rem',
                                cursor: 'pointer',
                                position: 'relative'
                            }}
                        >
                            🏷️
                        </button>
                        <button
                            className="notification-bell-btn"
                            onClick={() => navigate('/admin/support-messages')}
                            title={t('supportMessages')}
                            style={{
                                background: 'none',
                                border: 'none',
                                fontSize: '1.5rem',
                                cursor: 'pointer',
                                position: 'relative'
                            }}
                        >
                            🔔
                            {unreadMessagesCount > 0 && (
                                <span style={{
                                    position: 'absolute',
                                    top: '-5px',
                                    right: '-5px',
                                    backgroundColor: 'red',
                                    color: 'white',
                                    borderRadius: '50%',
                                    width: '20px',
                                    height: '20px',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center',
                                    fontSize: '0.8rem',
                                    fontWeight: 'bold'
                                }}>
                                    {unreadMessagesCount}
                                </span>
                            )}
                        </button>
                    </div>
                    <p>{t('manageDashboard')}</p>
                </div>

                {error && (
                    <div className="error-banner">
                        <span>⚠️ {error}</span>
                        <button onClick={() => setError(null)}>×</button>
                    </div>
                )}

                <div className="stats-grid">
                    {loadingStats ? (
                        Array.from({ length: 7 }).map((_, i) => (
                            <div key={i} className="stat-card skeleton-card">
                                <div className="skeleton-icon"></div>
                                <div className="stat-info">
                                    <span className="skeleton-value"></span>
                                    <span className="skeleton-label"></span>
                                </div>
                            </div>
                        ))
                    ) : stats ? (
                        <>
                            <div className="stat-card">
                                <div className="stat-icon">👥</div>
                                <div className="stat-info">
                                    <span className="stat-value">{stats.totalUsers}</span>
                                    <span className="stat-label">{t('totalUsers')}</span>
                                </div>
                            </div>
                            <div className="stat-card">
                                <div className="stat-icon">🔐</div>
                                <div className="stat-info">
                                    <span className="stat-value">{stats.adminUsers}</span>
                                    <span className="stat-label">{t('admins')}</span>
                                </div>
                            </div>
                            <div className="stat-card">
                                <div className="stat-icon">🏨</div>
                                <div className="stat-info">
                                    <span className="stat-value">{stats.totalHotels}</span>
                                    <span className="stat-label">{t('hotels')}</span>
                                </div>
                            </div>
                            <div className="stat-card">
                                <div className="stat-icon">❤️</div>
                                <div className="stat-info">
                                    <span className="stat-value">{stats.totalFavorites}</span>
                                    <span className="stat-label">{t('favorites')}</span>
                                </div>
                            </div>
                            <div className="stat-card">
                                <div className="stat-icon">📅</div>
                                <div className="stat-info">
                                    <span className="stat-value">{stats.totalReservations}</span>
                                    <span className="stat-label">{t('reservations')}</span>
                                </div>
                            </div>
                            <div className="stat-card">
                                <div className="stat-icon">💬</div>
                                <div className="stat-info">
                                    <span className="stat-value">{stats.totalReviews || 0}</span>
                                    <span className="stat-label">{t('comments')}</span>
                                </div>
                            </div>
                            <div className="stat-card revenue">
                                <div className="stat-icon">💰</div>
                                <div className="stat-info">
                                    <span className="stat-value">${stats.totalRevenue?.toLocaleString() || 0}</span>
                                    <span className="stat-label">{t('totalRevenue')}</span>
                                </div>
                            </div>
                        </>
                    ) : null}
                </div>

                <div className="dashboard-content">
                    <div className="users-panel">
                        <div className="panel-tabs">
                            <button
                                className={`panel-tab ${activeDashboardTab === 'users' ? 'active' : ''}`}
                                onClick={() => setActiveDashboardTab('users')}
                            >
                                👥 {t('users')}
                            </button>
                            <button
                                className={`panel-tab ${activeDashboardTab === 'moderators' ? 'active' : ''}`}
                                onClick={() => setActiveDashboardTab('moderators')}
                            >
                                🛡️ {t('moderators') || 'Moderators'}
                            </button>
                            <button
                                className={`panel-tab ${activeDashboardTab === 'hotels' ? 'active' : ''}`}
                                onClick={() => setActiveDashboardTab('hotels')}
                            >
                                🏨 {t('hotels')} ({adminHotels.length})
                            </button>
                        </div>

                        {activeDashboardTab === 'users' && (
                            <>
                                <div className="panel-header">
                                    <input
                                        type="text"
                                        placeholder={t('searchUsers')}
                                        value={searchTerm}
                                        onChange={(e) => setSearchTerm(e.target.value)}
                                        className="search-input"
                                    />
                                </div>
                                <div className="users-list">
                                    {loadingUsers ? (
                                        Array.from({ length: 5 }).map((_, i) => (
                                            <div key={i} className="user-card skeleton-card">
                                                <div className="skeleton-avatar"></div>
                                                <div className="user-info" style={{ flex: 1 }}>
                                                    <span className="skeleton-text" style={{ width: '60%' }}></span>
                                                    <span className="skeleton-text" style={{ width: '80%' }}></span>
                                                </div>
                                            </div>
                                        ))
                                    ) : filteredUsers.map(u => (
                                        <div
                                            key={u.id}
                                            className={`user-card ${selectedUser === u.id ? 'selected' : ''}`}
                                            onClick={() => fetchUserDetails(u.id)}
                                        >
                                            <div className="user-avatar">
                                                {u.avatarUrl ? (
                                                    <img src={getAssetUrl(u.avatarUrl)} alt={u.firstName} />
                                                ) : (
                                                    <span>{u.firstName?.[0] || u.email?.[0] || '?'}</span>
                                                )}
                                            </div>
                                            <div className="user-info">
                                                <span className="user-name">
                                                    {u.firstName} {u.lastName}
                                                    {u.roles?.includes('SuperAdmin') && <span className="badge super">Super</span>}
                                                    {u.roles?.includes('Admin') && !u.roles?.includes('SuperAdmin') && <span className="badge admin">Admin</span>}
                                                </span>
                                                <span className="user-email">{u.email}</span>
                                            </div>
                                            <div className="user-stats">
                                                <span title="Favorites">❤️ {u.favoritesCount}</span>
                                                <span title="Reservations">📅 {u.reservationsCount}</span>
                                                <span title="Comments">💬 {u.reviewsCount || 0}</span>
                                                <span title="Hotels Created">🏨 {u.hotelsCreated}</span>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </>
                        )}

                        {activeDashboardTab === 'moderators' && (
                            <>
                                <div className="panel-header">
                                    <input
                                        type="text"
                                        placeholder={t('searchModerators') || 'Search moderators...'}
                                        value={searchModeratorTerm}
                                        onChange={(e) => setSearchModeratorTerm(e.target.value)}
                                        className="search-input"
                                    />
                                </div>
                                <div className="moderators-list">
                                    {loadingModerators ? (
                                        Array.from({ length: 3 }).map((_, i) => (
                                            <div key={i} className="moderator-card-admin skeleton-card">
                                                <div className="moderator-info" style={{ flex: 1 }}>
                                                    <span className="skeleton-text" style={{ width: '50%' }}></span>
                                                    <span className="skeleton-text" style={{ width: '70%' }}></span>
                                                </div>
                                            </div>
                                        ))
                                    ) : filteredModerators.length === 0 ? (
                                        <p className="empty-message">{t('noModerators') || 'No moderators assigned'}</p>
                                    ) : (
                                        filteredModerators.map(m => (
                                            <div key={m.id} className="moderator-card-admin">
                                                <div className="moderator-info">
                                                    <strong>{m.userFullName}</strong>
                                                    <span>{m.userEmail}</span>
                                                    <small>{t('moderates') || 'Moderates'}: {m.hotelName}</small>
                                                </div>
                                                <button
                                                    className="btn-revoke"
                                                    onClick={() => handleRemoveModeratorRole(m.id)}
                                                    title={t('revokeModerator') || 'Revoke'}
                                                >
                                                    ❌
                                                </button>
                                            </div>
                                        ))
                                    )}
                                </div>
                            </>
                        )}
                        {activeDashboardTab === 'hotels' && (
                            <>
                                <div className="panel-header">
                                    <input
                                        type="text"
                                        placeholder={t('searchHotelsPlaceholder')}
                                        value={searchHotelTerm}
                                        onChange={(e) => setSearchHotelTerm(e.target.value)}
                                        className="search-input"
                                    />
                                </div>
                                <div className="hotels-admin-list">
                                    {loadingHotels ? (
                                        Array.from({ length: 4 }).map((_, i) => (
                                            <div key={i} className="hotel-admin-card skeleton-card">
                                                <div className="hotel-admin-info" style={{ flex: 1 }}>
                                                    <span className="skeleton-text" style={{ width: '55%' }}></span>
                                                    <span className="skeleton-text" style={{ width: '40%' }}></span>
                                                </div>
                                            </div>
                                        ))
                                    ) : filteredAdminHotels.length === 0 ? (
                                        <p className="empty-message">{t('noHotels')}</p>
                                    ) : (
                                        filteredAdminHotels.map(h => (
                                            <div key={h.id} className={`hotel-admin-card ${h.isSuspendedBySuperAdmin ? 'suspended' : ''}`}>
                                                <div className="hotel-admin-info">
                                                    <strong>{h.name}</strong>
                                                    <span>📍 {h.city}, {h.country}</span>
                                                    <small>👤 {h.ownerEmail || t('unknown')}</small>
                                                    {h.isSuspendedBySuperAdmin && (
                                                        <span className="badge-suspended">⛔ {t('suspended')}</span>
                                                    )}
                                                </div>
                                                <button
                                                    className={`btn-suspend ${h.isSuspendedBySuperAdmin ? 'btn-activate' : 'btn-deactivate'}`}
                                                    onClick={() => toggleHotelSuspend(h.id)}
                                                    title={h.isSuspendedBySuperAdmin ? t('activate') : t('deactivate')}
                                                >
                                                    {h.isSuspendedBySuperAdmin ? `✅ ${t('activate')}` : `🚫 ${t('deactivate')}`}
                                                </button>
                                            </div>
                                        ))
                                    )}
                                </div>
                            </>
                        )}

                    </div>

                    {showPromoCodes ? (
                        <div className="details-panel" style={{ overflowY: 'auto', position: 'relative' }}>
                            <button
                                className="close-btn"
                                onClick={() => setShowPromoCodes(false)}
                                style={{ position: 'absolute', top: '20px', right: '20px', zIndex: 10 }}
                            >
                                ×
                            </button>
                            <GlobalPromoCodes />
                        </div>
                    ) : userDetails ? (
                        <div className="details-panel">
                            <div className="panel-header">
                                <h2>{t('userDetails')}</h2>
                                <button className="close-btn" onClick={() => { setSelectedUser(null); setUserDetails(null); }}>×</button>
                            </div>
                            <div className="user-profile">
                                <div className="profile-avatar">
                                    {userDetails.avatarUrl ? (
                                        <img src={getAssetUrl(userDetails.avatarUrl)} alt={userDetails.firstName} />
                                    ) : (
                                        <span>{userDetails.firstName?.[0] || userDetails.email?.[0] || '?'}</span>
                                    )}
                                </div>
                                <div className="profile-info">
                                    <div className="profile-name-row">
                                        <h3>{userDetails.firstName} {userDetails.lastName}</h3>
                                        <div className="profile-actions">
                                            {userDetails.roles?.includes('SuperAdmin') ? (
                                                <button className="btn-demote-super" onClick={() => demoteSuperAdmin(userDetails.id)}>
                                                    {t('removeSuperAdmin')}
                                                </button>
                                            ) : (
                                                <>
                                                    {userDetails.roles?.includes('Admin') ? (
                                                        <button className="btn-demote" onClick={() => demoteUser(userDetails.id)}>
                                                            {t('removeAdmin')}
                                                        </button>
                                                    ) : (
                                                        <button className="btn-promote" onClick={() => promoteUser(userDetails.id)}>
                                                            {t('promoteToAdmin')}
                                                        </button>
                                                    )}
                                                    <button className="btn-promote-super" onClick={() => promoteSuperAdmin(userDetails.id)}>
                                                        {t('promoteToSuperAdmin')}
                                                    </button>
                                                </>
                                            )}
                                            {userDetails.isBlocked ? (
                                                <button className="btn-unblock" onClick={() => unblockUser(userDetails.id)}>
                                                    {t('unblockUser')}
                                                </button>
                                            ) : (
                                                !userDetails.roles?.includes('SuperAdmin') && (
                                                    <button className="btn-admin-block" onClick={() => blockUser(userDetails.id)}>
                                                        {t('blockUser')}
                                                    </button>
                                                )
                                            )}
                                            {!userDetails.roles?.includes('SuperAdmin') && (
                                                <button className="btn-delete-user" onClick={() => deleteUser(userDetails.id)}>
                                                    {t('deleteUser') || 'Изтрий'}
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                    <p>{userDetails.email}</p>
                                    <div className="profile-badges">
                                        {userDetails.roles?.map(role => (
                                            <span key={role} className={`badge ${role.toLowerCase()}`}>{role}</span>
                                        ))}
                                    </div>
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>💰 {t('totalSpent')}: ${userDetails.totalSpent?.toLocaleString() || 0}</h4>
                            </div>

                            <div className="details-section">
                                <h4>❤️ {t('favorites')} ({userDetails.favorites?.length || 0})</h4>
                                <div className="items-list">
                                    {userDetails.favorites?.length > 0 ? (
                                        userDetails.favorites.map(f => (
                                            <div key={f.id} className="item-card">
                                                <img src={f.hotelImageUrl || '/placeholder-hotel.jpg'} alt={f.hotelName} />
                                                <div className="item-info">
                                                    <span className="item-name">{f.hotelName}</span>
                                                    <span className="item-location">{f.hotelCity}, {f.hotelCountry}</span>
                                                </div>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="empty-message">{t('noFavorites')}</p>
                                    )}
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>📅 {t('reservations')} ({userDetails.reservations?.length || 0})</h4>
                                <div className="items-list">
                                    {userDetails.reservations?.length > 0 ? (
                                        userDetails.reservations.map(r => (
                                            <div key={r.id} className="item-card reservation">
                                                <img src={r.hotelImageUrl || '/placeholder-hotel.jpg'} alt={r.hotelName} />
                                                <div className="item-info">
                                                    <span className="item-name">{r.hotelName}</span>
                                                    <span className="item-dates">
                                                        {new Date(r.checkInDate).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')} - {new Date(r.checkOutDate).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')}
                                                    </span>
                                                    <span className={`item-status status-${r.status.toLowerCase()}`}>{getStatusLabel(r.status)}</span>
                                                </div>
                                                <span className="item-price">${r.totalPrice}</span>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="empty-message">{t('noReservations')}</p>
                                    )}
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>🏨 {t('hotelsCreated')} ({userDetails.hotelsCreated?.length || 0})</h4>
                                <div className="items-list">
                                    {userDetails.hotelsCreated?.length > 0 ? (
                                        userDetails.hotelsCreated.map(h => (
                                            <div key={h.id} className="item-card">
                                                <img src={h.imageUrl || '/placeholder-hotel.jpg'} alt={h.name} />
                                                <div className="item-info">
                                                    <span className="item-name">{h.name}</span>
                                                    <span className="item-location">{h.city}, {h.country}</span>
                                                </div>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="empty-message">{t('noHotelsCreated')}</p>
                                    )}
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>💬 {t('comments')} ({userDetails.reviews?.length || 0})</h4>
                                <div className="items-list">
                                    {userDetails.reviews?.length > 0 ? (
                                        userDetails.reviews.map(r => (
                                            <div key={r.id} className="item-card review">
                                                <div className="review-content" style={{ width: '100%' }}>
                                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                                        <div style={{ display: 'flex', flexDirection: 'column', gap: '4px', flex: 1 }}>
                                                            <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                                                                <span className="review-rating" style={{ color: '#f59e0b' }}>{'★'.repeat(r.rating)}</span>
                                                                <span className="review-hotel" style={{ fontWeight: '600' }}>{r.hotelName}</span>
                                                            </div>
                                                            <span className="review-text">{r.reviewText}</span>
                                                            <span className="review-date" style={{ fontSize: '0.8rem', color: '#64748b' }}>
                                                                {new Date(r.createdAt).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')}
                                                            </span>
                                                        </div>
                                                        <button
                                                            className="btn-delete-review"
                                                            onClick={() => handleDeleteReview(r.hotelId, r.id)}
                                                            style={{ padding: '4px 8px', backgroundColor: '#ef4444', color: 'white', borderRadius: '4px', border: 'none', cursor: 'pointer', fontSize: '0.8rem' }}
                                                        >
                                                            {t('delete') || 'Изтрий'}
                                                        </button>
                                                    </div>
                                                </div>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="empty-message">{t('noComments')}</p>
                                    )}
                                </div>
                            </div>

                            {/* Actual technical comments / activity if needed */}
                            {userDetails.comments?.length > 0 && (
                                <div className="details-section">
                                    <h4>💬 {t('activity') || 'Активност (Дискусии)'} ({userDetails.comments.length})</h4>
                                    <div className="items-list">
                                        {userDetails.comments.map(c => (
                                            <div key={c.id} className="item-card comment">
                                                <div className="comment-content" style={{ width: '100%' }}>
                                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                                        <div style={{ flex: 1 }}>
                                                            <span className="comment-text">
                                                                {c.parentCommentId && <span className="badge reply" style={{ marginRight: '8px', verticalAlign: 'middle', backgroundColor: '#64748b', color: 'white', padding: '2px 6px', borderRadius: '4px', fontSize: '0.7rem' }}>{t('reply') || 'Отговор'}</span>}
                                                                {c.text}
                                                            </span>
                                                            <span className="comment-hotel" style={{ display: 'block', fontSize: '0.85rem', color: '#64748b' }}>{t('onHotel')}: {c.hotelName}</span>
                                                        </div>
                                                        <button
                                                            className="btn-delete-review"
                                                            onClick={() => handleDeleteComment(c.id)}
                                                            style={{ marginLeft: '8px', padding: '4px 8px', backgroundColor: '#ef4444', color: 'white', borderRadius: '4px', border: 'none', cursor: 'pointer', fontSize: '0.8rem' }}
                                                        >
                                                            {t('delete') || 'Изтрий'}
                                                        </button>
                                                    </div>
                                                    <div className="comment-date" style={{ fontSize: '0.75rem', color: '#94a3b8', marginTop: '4px' }}>
                                                        {new Date(c.createdAt).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')}
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </div>
                    ) : null}
                </div>
            </div>
        </>
    );
};

export default AdminDashboard;
