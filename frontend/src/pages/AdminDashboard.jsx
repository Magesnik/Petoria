import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import './AdminDashboard.css';

const AdminDashboard = () => {
    const { user, isSuperAdmin } = useAuth();
    const { t } = useLanguage();
    const navigate = useNavigate();
    const [users, setUsers] = useState([]);
    const [stats, setStats] = useState(null);
    const [selectedUser, setSelectedUser] = useState(null);
    const [userDetails, setUserDetails] = useState(null);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [error, setError] = useState(null);

    useEffect(() => {
        if (!isSuperAdmin()) {
            navigate('/');
            return;
        }
        fetchData();
    }, [isSuperAdmin, navigate]);

    const fetchData = async () => {
        try {
            const token = localStorage.getItem('token');
            const headers = { 'Authorization': `Bearer ${token}` };

            const [usersRes, statsRes] = await Promise.all([
                fetch('http://localhost:5150/api/admin/users', { headers }),
                fetch('http://localhost:5150/api/admin/stats', { headers })
            ]);

            if (!usersRes.ok || !statsRes.ok) {
                throw new Error('Failed to fetch data');
            }

            setUsers(await usersRes.json());
            setStats(await statsRes.json());
            setLoading(false);
        } catch (err) {
            setError(err.message);
            setLoading(false);
        }
    };

    const fetchUserDetails = async (userId) => {
        try {
            const token = localStorage.getItem('token');
            const res = await fetch(`http://localhost:5150/api/admin/users/${userId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) throw new Error('Failed to fetch user details');
            const data = await res.json();
            setUserDetails(data);
            setSelectedUser(userId);
        } catch (err) {
            setError(err.message);
        }
    };

    const promoteUser = async (userId) => {
        try {
            const token = localStorage.getItem('token');
            const res = await fetch(`http://localhost:5150/api/admin/users/${userId}/promote`, {
                method: 'PUT',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) {
                const error = await res.json();
                throw new Error(error.message || 'Failed to promote user');
            }
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
            const token = localStorage.getItem('token');
            const res = await fetch(`http://localhost:5150/api/admin/users/${userId}/demote`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) {
                const error = await res.json();
                throw new Error(error.message || 'Failed to demote user');
            }
            fetchData();
            if (selectedUser === userId) {
                fetchUserDetails(userId);
            }
        } catch (err) {
            setError(err.message);
        }
    };

    const filteredUsers = users.filter(u =>
        u.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        u.firstName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        u.lastName?.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (!isSuperAdmin()) {
        return null;
    }

    if (loading) {
        return (
            <>
                <Header />
                <div className="admin-dashboard loading">
                    <div className="loading-spinner"></div>
                    <p>Loading dashboard...</p>
                </div>
            </>
        );
    }

    return (
        <>
            <Header />
            <div className="admin-dashboard">
                <div className="dashboard-header">
                    <h1>🛡️ Super Admin Dashboard</h1>
                    <p>Manage users, view statistics, and control admin access</p>
                </div>

                {error && (
                    <div className="error-banner">
                        <span>⚠️ {error}</span>
                        <button onClick={() => setError(null)}>×</button>
                    </div>
                )}

                {stats && (
                    <div className="stats-grid">
                        <div className="stat-card">
                            <div className="stat-icon">👥</div>
                            <div className="stat-info">
                                <span className="stat-value">{stats.totalUsers}</span>
                                <span className="stat-label">Total Users</span>
                            </div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-icon">🔐</div>
                            <div className="stat-info">
                                <span className="stat-value">{stats.adminUsers}</span>
                                <span className="stat-label">Admins</span>
                            </div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-icon">🏨</div>
                            <div className="stat-info">
                                <span className="stat-value">{stats.totalHotels}</span>
                                <span className="stat-label">Hotels</span>
                            </div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-icon">❤️</div>
                            <div className="stat-info">
                                <span className="stat-value">{stats.totalFavorites}</span>
                                <span className="stat-label">Favorites</span>
                            </div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-icon">📅</div>
                            <div className="stat-info">
                                <span className="stat-value">{stats.totalReservations}</span>
                                <span className="stat-label">Reservations</span>
                            </div>
                        </div>
                        <div className="stat-card">
                            <div className="stat-icon">💬</div>
                            <div className="stat-info">
                                <span className="stat-value">{stats.totalComments}</span>
                                <span className="stat-label">Comments</span>
                            </div>
                        </div>
                        <div className="stat-card revenue">
                            <div className="stat-icon">💰</div>
                            <div className="stat-info">
                                <span className="stat-value">${stats.totalRevenue?.toLocaleString() || 0}</span>
                                <span className="stat-label">Total Revenue</span>
                            </div>
                        </div>
                    </div>
                )}

                <div className="dashboard-content">
                    <div className="users-panel">
                        <div className="panel-header">
                            <h2>👥 Users</h2>
                            <input
                                type="text"
                                placeholder="Search users..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="search-input"
                            />
                        </div>
                        <div className="users-list">
                            {filteredUsers.map(u => (
                                <div
                                    key={u.id}
                                    className={`user-card ${selectedUser === u.id ? 'selected' : ''}`}
                                    onClick={() => fetchUserDetails(u.id)}
                                >
                                    <div className="user-avatar">
                                        {u.avatarUrl ? (
                                            <img src={`http://localhost:5150${u.avatarUrl}`} alt={u.firstName} />
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
                                        <span title="Comments">💬 {u.commentsCount}</span>
                                        <span title="Hotels Created">🏨 {u.hotelsCreated}</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    {userDetails && (
                        <div className="details-panel">
                            <div className="panel-header">
                                <h2>User Details</h2>
                                <button className="close-btn" onClick={() => { setSelectedUser(null); setUserDetails(null); }}>×</button>
                            </div>
                            <div className="user-profile">
                                <div className="profile-avatar">
                                    {userDetails.avatarUrl ? (
                                        <img src={`http://localhost:5150${userDetails.avatarUrl}`} alt={userDetails.firstName} />
                                    ) : (
                                        <span>{userDetails.firstName?.[0] || userDetails.email?.[0] || '?'}</span>
                                    )}
                                </div>
                                <div className="profile-info">
                                    <h3>{userDetails.firstName} {userDetails.lastName}</h3>
                                    <p>{userDetails.email}</p>
                                    <div className="profile-badges">
                                        {userDetails.roles?.map(role => (
                                            <span key={role} className={`badge ${role.toLowerCase()}`}>{role}</span>
                                        ))}
                                    </div>
                                </div>
                                <div className="profile-actions">
                                    {!userDetails.roles?.includes('SuperAdmin') && (
                                        <>
                                            {userDetails.roles?.includes('Admin') ? (
                                                <button className="btn-demote" onClick={() => demoteUser(userDetails.id)}>
                                                    Remove Admin
                                                </button>
                                            ) : (
                                                <button className="btn-promote" onClick={() => promoteUser(userDetails.id)}>
                                                    Promote to Admin
                                                </button>
                                            )}
                                        </>
                                    )}
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>💰 Total Spent: ${userDetails.totalSpent?.toLocaleString() || 0}</h4>
                            </div>

                            <div className="details-section">
                                <h4>❤️ Favorites ({userDetails.favorites?.length || 0})</h4>
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
                                        <p className="empty-message">No favorites yet</p>
                                    )}
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>📅 Reservations ({userDetails.reservations?.length || 0})</h4>
                                <div className="items-list">
                                    {userDetails.reservations?.length > 0 ? (
                                        userDetails.reservations.map(r => (
                                            <div key={r.id} className="item-card reservation">
                                                <img src={r.hotelImageUrl || '/placeholder-hotel.jpg'} alt={r.hotelName} />
                                                <div className="item-info">
                                                    <span className="item-name">{r.hotelName}</span>
                                                    <span className="item-dates">
                                                        {new Date(r.checkInDate).toLocaleDateString()} - {new Date(r.checkOutDate).toLocaleDateString()}
                                                    </span>
                                                    <span className={`item-status status-${r.status.toLowerCase()}`}>{r.status}</span>
                                                </div>
                                                <span className="item-price">${r.totalPrice}</span>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="empty-message">No reservations yet</p>
                                    )}
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>🏨 Hotels Created ({userDetails.hotelsCreated?.length || 0})</h4>
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
                                        <p className="empty-message">No hotels created</p>
                                    )}
                                </div>
                            </div>

                            <div className="details-section">
                                <h4>💬 Comments ({userDetails.comments?.length || 0})</h4>
                                <div className="items-list">
                                    {userDetails.comments?.length > 0 ? (
                                        userDetails.comments.map(c => (
                                            <div key={c.id} className="item-card comment">
                                                <div className="comment-content">
                                                    <span className="comment-text">{c.text}</span>
                                                    <span className="comment-hotel">On: {c.hotelName}</span>
                                                    <span className="comment-date">
                                                        {new Date(c.createdAt).toLocaleDateString()}
                                                    </span>
                                                </div>
                                                <div className="comment-stats">
                                                    <span>👍 {c.likesCount}</span>
                                                    <span>👎 {c.dislikesCount}</span>
                                                    <span>💬 {c.repliesCount}</span>
                                                </div>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="empty-message">No comments yet</p>
                                    )}
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};

export default AdminDashboard;
