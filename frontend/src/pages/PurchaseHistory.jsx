import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import './PurchaseHistory.css';

const PurchaseHistory = () => {
    const { t } = useLanguage();
    const [purchases, setPurchases] = useState([]);

    useEffect(() => {
        // TODO: Fetch purchase history from API
        // For now, using empty array as placeholder
        setPurchases([]);
    }, []);

    const getStatusClass = (status) => {
        switch (status.toLowerCase()) {
            case 'confirmed':
                return 'status-confirmed';
            case 'pending':
                return 'status-pending';
            case 'cancelled':
                return 'status-cancelled';
            default:
                return '';
        }
    };

    return (
        <>
            <Header />
            <div className="purchase-history-page">
                <div className="purchase-hero">
                    <h1>{t('purchaseHistoryTitle')}</h1>
                    <p>{t('purchaseHistorySubtitle')}</p>
                </div>

                <div className="purchase-container">
                    {purchases.length === 0 ? (
                        <div className="empty-state">
                            <div className="empty-icon">🛒</div>
                            <h2>{t('noPurchases')}</h2>
                            <p>{t('noPurchasesText')}</p>
                            <Link to="/hotels" className="btn btn-primary">
                                {t('exploreHotels')}
                            </Link>
                        </div>
                    ) : (
                        <div className="purchase-list">
                            {purchases.map((purchase) => (
                                <div key={purchase.id} className="purchase-card">
                                    <div className="purchase-image">
                                        <img src={purchase.hotelImageUrl} alt={purchase.hotelName} />
                                    </div>
                                    <div className="purchase-details">
                                        <h3>{purchase.hotelName}</h3>
                                        <p className="purchase-location">📍 {purchase.location}</p>
                                        <div className="purchase-info">
                                            <div className="info-item">
                                                <span className="info-label">{t('bookingDate')}:</span>
                                                <span className="info-value">
                                                    {new Date(purchase.bookingDate).toLocaleDateString()}
                                                </span>
                                            </div>
                                            <div className="info-item">
                                                <span className="info-label">{t('checkIn')}:</span>
                                                <span className="info-value">
                                                    {new Date(purchase.checkInDate).toLocaleDateString()}
                                                </span>
                                            </div>
                                            <div className="info-item">
                                                <span className="info-label">{t('checkOut')}:</span>
                                                <span className="info-value">
                                                    {new Date(purchase.checkOutDate).toLocaleDateString()}
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="purchase-summary">
                                        <div className="purchase-price">
                                            <span className="price-label">{t('totalPrice')}</span>
                                            <span className="price-value">${purchase.totalPrice}</span>
                                        </div>
                                        <div className="purchase-status">
                                            <span className={`status-badge ${getStatusClass(purchase.status)}`}>
                                                {t(purchase.status.toLowerCase())}
                                            </span>
                                        </div>
                                        <Link to={`/hotels/${purchase.hotelId}`} className="btn btn-view">
                                            View Hotel
                                        </Link>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};

export default PurchaseHistory;
