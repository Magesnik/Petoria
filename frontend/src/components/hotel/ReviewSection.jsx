import React, { useState, useEffect } from 'react';
import { api, getAssetUrl } from '../../utils/api';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import './ReviewSection.css';

/** Секция с отзиви — среден рейтинг, форма за оценка и списък с рецензии. */
const ReviewSection = ({ hotelId, hotelCreatedById }) => {
    const [reviews, setReviews] = useState([]);
    const [loading, setLoading] = useState(true);
    // Данни за потребителския отзив (рейтинг и текст)
    const [userReview, setUserReview] = useState({ rating: 5, reviewText: '' });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { user, isAdmin, isSuperAdmin } = useAuth();
    const { t, language } = useLanguage();
    // Изчислен среден рейтинг от всички отзиви
    const [averageRating, setAverageRating] = useState(0);

    const fetchReviews = React.useCallback(async () => {
        setLoading(true);
        try {
            const data = await api.get(`/hotels/${hotelId}/reviews`);
            setReviews(data);

            // Calculate average rating
            if (data.length > 0) {
                const avg = data.reduce((acc, curr) => acc + curr.rating, 0) / data.length;
                setAverageRating(avg);
            } else {
                setAverageRating(0);
            }
        } catch (err) {
            console.error('Error fetching reviews:', err);
        } finally {
            setLoading(false);
        }
    }, [hotelId]);

    useEffect(() => {
        fetchReviews();
    }, [fetchReviews]);

    const handleSubmitReview = async (e) => {
        e.preventDefault();
        if (!user) {
            alert('Please login to leave a review');
            return;
        }

        setIsSubmitting(true);
        try {
            await api.post(`/hotels/${hotelId}/reviews`, userReview);

            await fetchReviews();
            setUserReview({ rating: 5, reviewText: '' });
            alert('Review submitted successfully!');
        } catch (err) {
            alert(err.message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDeleteReview = async (reviewId) => {
        if (!window.confirm('Are you sure you want to delete this review?')) return;

        try {
            await api.delete(`/hotels/${hotelId}/reviews/${reviewId}`);
            fetchReviews();
        } catch (err) {
            alert(err.message);
        }
    };

    const formatDate = (dateString) => {
        const locale = language === 'bg' ? 'bg-BG' : 'en-US';
        return new Date(dateString).toLocaleDateString(locale, {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    };

    return (
        <div className="review-section">
            <h2 className="section-title">
                {t('reviews')}
                {reviews.length > 0 && <span className="review-count">({reviews.length})</span>}
            </h2>

            {/* Rating Summary */}
            <div className="rating-summary">
                <div className="average-rating">
                    <span className="rating-number">{averageRating.toFixed(1)}</span>
                    <div className="rating-stars">
                        {'★'.repeat(Math.round(averageRating))}
                        {'☆'.repeat(5 - Math.round(averageRating))}
                    </div>
                    <span className="rating-text">{t('averageRating')}</span>
                </div>

                {user && (
                    <div className="write-review-box">
                        <h3>{t('writeReview')}</h3>
                        <form onSubmit={handleSubmitReview}>
                            <div className="rating-input">
                                <label>{t('yourRating')}:</label>
                                {[1, 2, 3, 4, 5].map((star) => (
                                    <button
                                        key={star}
                                        type="button"
                                        className={`star-btn ${userReview.rating >= star ? 'active' : ''}`}
                                        onClick={() => setUserReview({ ...userReview, rating: star })}
                                    >
                                        ★
                                    </button>
                                ))}
                            </div>
                            <textarea
                                value={userReview.reviewText}
                                onChange={(e) => setUserReview({ ...userReview, reviewText: e.target.value })}
                                placeholder={t('shareYourExperience')}
                                required
                                maxLength={500}
                            />
                            <button type="submit" disabled={isSubmitting} className="btn-submit">
                                {isSubmitting ? t('loading') : t('submitReview')}
                            </button>
                        </form>
                    </div>
                )}
            </div>

            {/* Reviews List */}
            <div className="reviews-list">
                {loading ? (
                    <div className="loading">{t('loading')}</div>
                ) : reviews.length === 0 ? (
                    <div className="no-reviews">{t('noReviews')}</div>
                ) : (
                    reviews.map((review) => (
                        <div key={review.id} className="review-card">
                            <div className="review-header">
                                <div className="reviewer-info">
                                    <div className="reviewer-avatar">
                                        {review.user.avatarUrl ? (
                                            <img
                                                src={getAssetUrl(review.user.avatarUrl)}
                                                alt={review.user.firstName}
                                            />
                                        ) : (
                                            <span>{review.user.firstName?.[0] || 'U'}</span>
                                        )}
                                    </div>
                                    <div>
                                        <div className="reviewer-name">
                                            {review.user.firstName} {review.user.lastName}
                                        </div>
                                        <div className="review-date">{formatDate(review.createdAt)}</div>
                                    </div>
                                </div>
                                <div className="review-rating">
                                    {'★'.repeat(review.rating)}
                                    {'☆'.repeat(5 - review.rating)}
                                </div>
                            </div>
                            <p className="review-text">{review.reviewText}</p>

                            {(() => {
                                const isReviewOwner = user?.id === review.user.id;
                                const isSuper = isSuperAdmin && isSuperAdmin();
                                const isHotelOwnerAdmin = isAdmin && isAdmin() && user?.id === hotelCreatedById;
                                return (isReviewOwner || isSuper || isHotelOwnerAdmin) && (
                                    <button
                                        onClick={() => handleDeleteReview(review.id)}
                                        className="btn-delete-review"
                                    >
                                        {t('delete')}
                                    </button>
                                );
                            })()}
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

export default ReviewSection;
