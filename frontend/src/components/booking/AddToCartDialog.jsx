import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useLanguage } from '../../context/LanguageContext';
import { useCurrency } from '../../context/CurrencyContext';
import './AddToCartDialog.css';

const AddToCartDialog = ({ item, onClose }) => {
    const navigate = useNavigate();
    const { t } = useLanguage();
    const { convertAndFormat } = useCurrency();

    const handleViewCart = () => {
        onClose();
        navigate('/cart');
    };

    return (
        <div className="atc-overlay" onClick={onClose}>
            <div className="atc-dialog" onClick={e => e.stopPropagation()}>
                <div className="atc-check">✓</div>
                <h3 className="atc-title">{t('addedToCart')}</h3>
                <div className="atc-item-info">
                    {item?.hotelImage && (
                        <img src={item.hotelImage} alt={item.hotelName} className="atc-hotel-img" />
                    )}
                    <div className="atc-item-details">
                        <p className="atc-hotel-name">{item?.hotelName}</p>
                        <p className="atc-room-name">{item?.roomTypeName}</p>
                        <p className="atc-dates">
                            {item?.checkInDate} → {item?.checkOutDate}
                        </p>
                        {item?.priceInfo && (
                            <p className="atc-price">{convertAndFormat(item.priceInfo.totalPrice)}</p>
                        )}
                    </div>
                </div>
                <div className="atc-actions">
                    <button className="atc-btn-continue" onClick={onClose}>
                        {t('continueBrowsing')}
                    </button>
                    <button className="atc-btn-cart" onClick={handleViewCart}>
                        🛒 {t('viewCart')}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default AddToCartDialog;
