import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import './ReviewSection.css';

const ReviewSection = ({ hotelId }) => {
    const [reviews, setReviews] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [userReview, setUserReview] = useState({ rating: 5, reviewText: '' });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { user } = useAuth();
    const [averageRating, setAverageRating] = useState(0);

    useEffect(() => {
        fetchReviews();
    }, [hotelId]);

    const fetchReviews = async () => {
        setLoading(true);
        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${hotelId}/reviews`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            });
            if (response.ok) {
                const data = await response.json();
                setReviews(data);

                // Calculate average rating
                if (data.length > 0) {
                    const avg = data.reduce((acc, curr) => acc + curr.rating, 0) / data.length;
                    setAverageRating(avg);
                } else {
                    setAverageRating(0);
                }
            } else {
                throw new Error('Failed to fetch reviews');
            }
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const handleSubmitReview = async (e) => {
        e.preventDefault();
        if (!user) {
            alert('Please login to leave a review');
            return;
        }

        setIsSubmitting(true);
        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${hotelId}/reviews`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify(userReview)
            });

            if (response.ok) {
                await fetchReviews();
                setUserReview({ rating: 5, reviewText: '' });
                alert('Review submitted successfully!');
            } else {
                throw new Error('Failed to submit review');
            }
        } catch (err) {
            alert(err.message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDeleteReview = async (reviewId) => {
        if (!window.confirm('Are you sure you want to delete this review?')) return;

        try {
            const response = await fetch(`http://localhost:5150/api/hotels/${hotelId}/reviews/${reviewId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            });

            if (response.ok) {
                fetchReviews();
            } else {
                throw new Error('Failed to delete review');
            }
        } catch (err) {
            alert(err.message);
        }
    };

    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleDateString('bg-BG', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    };

    return (
        <div className="review-section">
            <h2 className="section-title">
                Мнения на гости
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
                    <span className="rating-text">средна оценка</span>
                </div>

                {user && (
                    <div className="write-review-box">
                        <h3>Оценете престоя си</h3>
                        <form onSubmit={handleSubmitReview}>
                            <div className="rating-input">
                                <label>Оценка:</label>
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
                                placeholder="Споделете впечатленията си..."
                                required
                            />
                            <button type="submit" disabled={isSubmitting} className="btn-submit">
                                {isSubmitting ? 'Изпращане...' : 'Публикувай мнение'}
                            </button>
                        </form>
                    </div>
                )}
            </div>

            {/* Reviews List */}
            <div className="reviews-list">
                {loading ? (
                    <div className="loading">Зареждане на мнения...</div>
                ) : reviews.length === 0 ? (
                    <div className="no-reviews">Все още няма мнения за този хотел.</div>
                ) : (
                    reviews.map((review) => (
                        <div key={review.id} className="review-card">
                            <div className="review-header">
                                <div className="reviewer-info">
                                    <div className="reviewer-avatar">
                                        {review.user.avatarUrl ? (
                                            <img
                                                src={review.user.avatarUrl.startsWith('http')
                                                    ? review.user.avatarUrl
                                                    : `http://localhost:5150${review.user.avatarUrl}`
                                                }
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

                            {(user?.id === review.user.id || user?.role === 'Admin' || user?.role === 'SuperAdmin') && (
                                <button
                                    onClick={() => handleDeleteReview(review.id)}
                                    className="btn-delete-review"
                                >
                                    Изтрий
                                </button>
                            )}
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

export default ReviewSection;
