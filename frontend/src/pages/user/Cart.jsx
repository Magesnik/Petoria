import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from '../../context/CartContext';
import { useLanguage } from '../../context/LanguageContext';
import { useCurrency } from '../../context/CurrencyContext';
import { useAuth } from '../../context/AuthContext';
import { api } from '../../utils/api';
import './Cart.css';

const Cart = () => {
    const { cartItems, removeFromCart, clearCart, cartLoading } = useCart();
    const { t } = useLanguage();
    const { convertAndFormat } = useCurrency();
    const { user } = useAuth();
    const navigate = useNavigate();
    const [checkoutLoading, setCheckoutLoading] = useState(false);
    const [checkoutError, setCheckoutError] = useState('');
    const [checkoutSuccess, setCheckoutSuccess] = useState('');
    const [promoCode, setPromoCode] = useState('');
    const [promoApplied, setPromoApplied] = useState(false);
    const [promoError, setPromoError] = useState('');

    // API response uses item.totalPrice directly (not nested in priceInfo)
    const total = cartItems.reduce((sum, item) => sum + (item.totalPrice || 0), 0);

    const formatDate = (dateStr) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString();
    };

    const handleCheckout = async () => {
        if (!user) {
            navigate('/login');
            return;
        }
        setCheckoutLoading(true);
        setCheckoutError('');
        try {
            // Store cart items in sessionStorage so PaymentSuccess can confirm them
            sessionStorage.setItem('pendingCartItems', JSON.stringify(
                cartItems.map(item => ({
                    hotelId: item.hotelId,
                    roomTypeId: item.roomTypeId,
                    checkInDate: item.checkInDate,
                    checkOutDate: item.checkOutDate,
                    numberOfRooms: item.numberOfRooms,
                }))
            ));

            const data = await api.post('/stripe/create-checkout-session', {
                items: cartItems.map(item => ({
                    cartItemId: item.id,
                    hotelId: item.hotelId,
                    hotelName: item.hotelName,
                    roomTypeId: item.roomTypeId,
                    roomTypeName: item.roomTypeName,
                    checkInDate: item.checkInDate,
                    checkOutDate: item.checkOutDate,
                    numberOfRooms: item.numberOfRooms,
                    totalPrice: item.totalPrice,
                })),
                frontendBaseUrl: window.location.origin,
            });

            // Redirect to Stripe Checkout
            window.location.href = data.url;
        } catch (err) {
            setCheckoutError(err.message || t('checkoutError'));
            setCheckoutLoading(false);
        }
    };


    if (cartLoading) {
        return (
            <div className="cart-page">
                <div className="cart-container">
                    <div className="cart-loading">⏳ {t('loading')}</div>
                </div>
            </div>
        );
    }

    return (
        <div className="cart-page">
            <div className="cart-container">
                <div className="cart-header">
                    <h1 className="cart-title">🛒 {t('cartTitle')}</h1>
                    {cartItems.length > 0 && (
                        <span className="cart-count-badge">
                            {cartItems.length} {cartItems.length === 1 ? t('item') : t('items')}
                        </span>
                    )}
                </div>

                {checkoutError && <div className="cart-error">{checkoutError}</div>}
                {checkoutSuccess && <div className="cart-success">{checkoutSuccess}</div>}

                {cartItems.length === 0 ? (
                    <div className="cart-empty">
                        <div className="cart-empty-icon">🛒</div>
                        <h2>{t('cartEmpty')}</h2>
                        <p>{t('cartEmptyText')}</p>
                        <Link to="/hotels" className="cart-btn-browse">{t('browseHotels')}</Link>
                    </div>
                ) : (
                    <div className="cart-layout">
                        <div className="cart-items-list">
                            {cartItems.map(item => (
                                <div key={item.id} className="cart-item-card">
                                    {item.hotelImageUrl && (
                                        <img
                                            src={item.hotelImageUrl}
                                            alt={item.hotelName}
                                            className="cart-item-img"
                                        />
                                    )}
                                    <div className="cart-item-body">
                                        <div className="cart-item-top">
                                            <div>
                                                <h3 className="cart-item-hotel">{item.hotelName}</h3>
                                                <p className="cart-item-room">{item.roomTypeName}</p>
                                            </div>
                                            <button
                                                className="cart-remove-btn"
                                                onClick={() => removeFromCart(item.id)}
                                                title={t('removeFromCart')}
                                            >
                                                ✕
                                            </button>
                                        </div>
                                        <div className="cart-item-meta">
                                            <span className="cart-meta-pill">📅 {formatDate(item.checkInDate)} → {formatDate(item.checkOutDate)}</span>
                                            <span className="cart-meta-pill">🛏️ {item.numberOfRooms} {item.numberOfRooms === 1 ? t('room') : t('rooms')}</span>
                                            {item.numberOfNights > 0 && (
                                                <span className="cart-meta-pill">🌙 {item.numberOfNights} {t('nights')}</span>
                                            )}
                                        </div>
                                        <div className="cart-item-price-row">
                                            {item.originalPrice > item.totalPrice && (
                                                <span className="cart-item-original">
                                                    {convertAndFormat(item.originalPrice)}
                                                </span>
                                            )}
                                            <span className="cart-item-price">
                                                {convertAndFormat(item.totalPrice || 0)}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="cart-summary">
                            <h2 className="cart-summary-title">{t('orderSummary')}</h2>
                            <div className="cart-summary-rows">
                                {cartItems.map(item => (
                                    <div key={item.id} className="cart-summary-row">
                                        <span className="cart-summary-label">{item.hotelName}</span>
                                        <span>{convertAndFormat(item.totalPrice || 0)}</span>
                                    </div>
                                ))}
                            </div>
                            <div className="cart-summary-total">
                                <span>{t('total')}</span>
                                <span className="cart-total-price">{convertAndFormat(total)}</span>
                            </div>
                            <button
                                className="cart-checkout-btn"
                                onClick={handleCheckout}
                                disabled={checkoutLoading}
                            >
                                {checkoutLoading ? `⏳ ${t('booking')}...` : `✅ ${t('proceedToCheckout')}`}
                            </button>
                            <Link to="/hotels" className="cart-continue-link">{t('continueBrowsing')}</Link>

                            {/* Promo code */}
                            <div className="cart-promo">
                                <p className="cart-promo-label">🏷️ {t('promoCode') || 'Промо код'}</p>
                                <div className="cart-promo-row">
                                    <input
                                        className={`cart-promo-input${promoApplied ? ' applied' : ''}`}
                                        type="text"
                                        placeholder={t('enterPromoCode') || 'Въведи промо код...'}
                                        value={promoCode}
                                        onChange={e => {
                                            setPromoCode(e.target.value.toUpperCase());
                                            setPromoError('');
                                            setPromoApplied(false);
                                        }}
                                        disabled={promoApplied}
                                    />
                                    <button
                                        className="cart-promo-btn"
                                        disabled={!promoCode.trim() || promoApplied}
                                        onClick={() => {
                                            // TODO: connect to backend discount validation
                                            setPromoError(t('invalidPromoCode') || 'Невалиден промо код');
                                        }}
                                    >
                                        {promoApplied ? '✓' : (t('apply') || 'Добави')}
                                    </button>
                                </div>
                                {promoError && <p className="cart-promo-error">{promoError}</p>}
                                {promoApplied && <p className="cart-promo-success">✓ {t('promoApplied') || 'Промо кодът е приложен!'}</p>}
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Cart;
