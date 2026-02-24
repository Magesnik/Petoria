import React, { useState, useEffect } from 'react';
import { api } from '../../utils/api';
import { useLanguage } from '../../context/LanguageContext';
import './AdminDashboard.css'; // Reusing some admin dashboard styles or creating new ones

const GlobalPromoCodes = () => {
    const { t, language } = useLanguage();
    const [promoCodes, setPromoCodes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [isCreating, setIsCreating] = useState(false);

    // Form state
    const [code, setCode] = useState('');
    const [discountPercentage, setDiscountPercentage] = useState('');
    const [maxActivations, setMaxActivations] = useState(100);
    const [validDays, setValidDays] = useState(30);

    const fetchPromoCodes = async () => {
        try {
            setLoading(true);
            const data = await api.get('/promocodes/global');
            setPromoCodes(data);
            setError(null);
        } catch (err) {
            setError(err.message || 'Failed to fetch global promo codes');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchPromoCodes();
    }, []);

    const handleCreate = async (e) => {
        e.preventDefault();
        try {
            setError(null);
            await api.post('/promocodes/global', {
                code,
                discountPercentage: parseFloat(discountPercentage),
                maxActivations: parseInt(maxActivations),
                validDays: parseInt(validDays)
            });
            setIsCreating(false);
            setCode('');
            setDiscountPercentage('');
            setMaxActivations(100);
            setValidDays(30);
            fetchPromoCodes();
        } catch (err) {
            setError(err.message || 'Failed to create global promo code');
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm(t('confirmDeletePromo') || 'Are you sure you want to delete this global promo code?')) return;
        try {
            await api.delete(`/promocodes/${id}`);
            fetchPromoCodes();
        } catch (err) {
            setError(err.message || 'Failed to delete promo code');
        }
    };

    if (loading) return <div className="loading-spinner"></div>;

    return (
        <div className="global-promo-codes-panel">
            <div className="panel-header">
                <h2>🏷️ {t('globalPromoCodes')}</h2>
                <button className="btn-create" onClick={() => setIsCreating(!isCreating)}>
                    {isCreating ? t('cancel') : (t('createPromoCode') || '+ Create')}
                </button>
            </div>

            {error && <div className="error-banner"><span>⚠️ {error}</span><button onClick={() => setError(null)}>×</button></div>}

            {isCreating && (
                <div className="promo-create-form" style={{ background: 'var(--card-bg)', padding: '1.5rem', borderRadius: '12px', marginBottom: '1.5rem', border: '1px solid var(--border-color)' }}>
                    <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', color: 'var(--text-color)' }}>{t('codeExample')}</label>
                                <input
                                    type="text"
                                    value={code}
                                    onChange={e => setCode(e.target.value.toUpperCase())}
                                    required
                                    style={{ width: '100%', padding: '0.8rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--bg-color)', color: 'var(--text-color)' }}
                                />
                            </div>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', color: 'var(--text-color)' }}>{t('discountPercentageShort')}</label>
                                <input
                                    type="number"
                                    step="0.01"
                                    min="0.01"
                                    max="100"
                                    value={discountPercentage}
                                    onChange={e => setDiscountPercentage(e.target.value)}
                                    required
                                    style={{ width: '100%', padding: '0.8rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--bg-color)', color: 'var(--text-color)' }}
                                />
                            </div>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', color: 'var(--text-color)' }}>{t('maxActivations')}</label>
                                <input
                                    type="number"
                                    min="1"
                                    value={maxActivations}
                                    onChange={e => setMaxActivations(e.target.value)}
                                    required
                                    style={{ width: '100%', padding: '0.8rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--bg-color)', color: 'var(--text-color)' }}
                                />
                            </div>
                            <div>
                                <label style={{ display: 'block', marginBottom: '0.5rem', color: 'var(--text-color)' }}>{t('validDays')}</label>
                                <input
                                    type="number"
                                    min="1"
                                    value={validDays}
                                    onChange={e => setValidDays(e.target.value)}
                                    required
                                    style={{ width: '100%', padding: '0.8rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--bg-color)', color: 'var(--text-color)' }}
                                />
                            </div>
                        </div>
                        <button type="submit" style={{ padding: '0.8rem', background: 'var(--primary-color)', color: 'white', border: 'none', borderRadius: '8px', fontWeight: 'bold', cursor: 'pointer', marginTop: '1rem' }}>
                            {t('savePromoCode')}
                        </button>
                    </form>
                </div>
            )}

            <div className="promo-codes-list" style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                {promoCodes.length === 0 ? (
                    <p className="empty-message" style={{ textAlign: 'center', color: 'var(--text-muted)' }}>{t('noPromoCodes') || 'No global promo codes found'}</p>
                ) : (
                    promoCodes.map(promo => (
                        <div key={promo.id} className="promo-card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '1rem 1.5rem', background: 'var(--card-bg)', borderRadius: '12px', border: '1px solid var(--border-color)' }}>
                            <div className="promo-info">
                                <strong style={{ fontSize: '1.2rem', color: 'var(--primary-color)', display: 'block', marginBottom: '0.5rem' }}>{promo.code} <span style={{ fontSize: '0.9rem', color: 'var(--text-color)', background: 'rgba(59, 130, 246, 0.1)', padding: '0.2rem 0.5rem', borderRadius: '4px', marginLeft: '0.5rem' }}>{promo.discountPercentage}% {t('off')}</span></strong>
                                <div style={{ display: 'flex', gap: '1.5rem', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
                                    <span>{t('activations')}: {promo.currentActivations} / {promo.maxActivations}</span>
                                    <span>{t('expires')}: {new Date(promo.expirationDate).toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')}</span>
                                    <span style={{ color: promo.isActive ? 'green' : 'red', fontWeight: 'bold' }}>{promo.isActive ? t('active') : t('inactive')}</span>
                                </div>
                            </div>
                            <button className="btn-delete" onClick={() => handleDelete(promo.id)} style={{ background: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', border: 'none', padding: '0.5rem 1rem', borderRadius: '8px', cursor: 'pointer', fontWeight: 'bold' }}>
                                🗑️ {t('delete') || 'Delete'}
                            </button>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

export default GlobalPromoCodes;
