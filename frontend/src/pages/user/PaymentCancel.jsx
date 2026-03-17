import React from 'react';
import { useNavigate } from 'react-router-dom';
import './PaymentSuccess.css';

/** Страница при отменено плащане — информира потребителя и предлага навигация. */
const PaymentCancel = () => {
    const navigate = useNavigate();

    return (
        <div className="payment-result-page">
            <div className="payment-result-card">
                <div className="payment-icon cancel-icon">❌</div>
                <h2>Плащането е отменено</h2>
                <p>Не притеснявай се — нищо не е резервирано и не е взето от сметката ти.</p>
                <div className="payment-actions">
                    <button className="payment-btn" onClick={() => navigate('/cart')}>
                        ← Обратно към количката
                    </button>
                    <button className="payment-btn secondary" onClick={() => navigate('/hotels')}>
                        Разгледай хотели
                    </button>
                </div>
            </div>
        </div>
    );
};

export default PaymentCancel;
