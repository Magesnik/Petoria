import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../../utils/api';
import { useCart } from '../../context/CartContext';
import { useLanguage } from '../../context/LanguageContext';
import './PaymentSuccess.css';

const PaymentSuccess = () => {
    const navigate = useNavigate();
    const { clearCart } = useCart();
    const { t } = useLanguage();
    const [status, setStatus] = useState('confirming'); // 'confirming' | 'success' | 'error'
    const [errorMsg, setErrorMsg] = useState('');

    useEffect(() => {
        const confirm = async () => {
            try {
                const raw = sessionStorage.getItem('pendingCartItems');
                const appliedPromo = sessionStorage.getItem('appliedPromoCode');
                if (!raw) {
                    // No pending items — maybe user landed here directly
                    setStatus('success');
                    setTimeout(() => navigate('/purchase-history'), 3000);
                    return;
                }

                const items = JSON.parse(raw);
                await api.post('/reservations/confirm-cart', {
                    items,
                    promoCode: appliedPromo || null
                });
                sessionStorage.removeItem('pendingCartItems');
                sessionStorage.removeItem('appliedPromoCode');
                await clearCart();
                setStatus('success');
                setTimeout(() => navigate('/purchase-history'), 3500);
            } catch (err) {
                console.error('Failed to confirm reservations:', err);
                setErrorMsg(err.message || 'Грешка при потвърждаване на резервацията');
                setStatus('error');
            }
        };

        confirm();
    }, []); // eslint-disable-line react-hooks/exhaustive-deps

    return (
        <div className="payment-result-page">
            <div className="payment-result-card">
                {status === 'confirming' && (
                    <>
                        <div className="payment-spinner" />
                        <h2>Потвърждаване на резервацията...</h2>
                        <p>Моля изчакайте.</p>
                    </>
                )}
                {status === 'success' && (
                    <>
                        <div className="payment-icon success-icon">✅</div>
                        <h2>Плащането е успешно!</h2>
                        <p>Резервацията ти е потвърдена. Пренасочване към историята...</p>
                        <button className="payment-btn" onClick={() => navigate('/purchase-history')}>
                            Виж резервациите →
                        </button>
                    </>
                )}
                {status === 'error' && (
                    <>
                        <div className="payment-icon error-icon">⚠️</div>
                        <h2>Грешка при потвърждение</h2>
                        <p>{errorMsg}</p>
                        <p>Плащането ти беше получено — свържи се с поддръжка.</p>
                        <button className="payment-btn" onClick={() => navigate('/purchase-history')}>
                            Моите резервации
                        </button>
                    </>
                )}
            </div>
        </div>
    );
};

export default PaymentSuccess;
