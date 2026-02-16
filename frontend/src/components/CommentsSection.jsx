import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { useAuth } from '../context/AuthContext';
import Comment from './Comment';
import CommentForm from './CommentForm';
import './CommentsSection.css';

const CommentsSection = ({ hotelId }) => {
    const [comments, setComments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const { user } = useAuth();

    useEffect(() => {
        fetchComments();
    }, [hotelId]);

    const fetchComments = async () => {
        setLoading(true);
        try {
            const data = await api.get(`/hotels/${hotelId}/comments`);
            setComments(data);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching comments:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleCommentAdded = (newComment) => {
        setComments([newComment, ...comments]);
    };

    const handleCommentDeleted = (commentId) => {
        setComments(comments.filter(c => c.id !== commentId));
    };

    const handleRatingUpdated = (commentId, updatedRatings) => {
        setComments(comments.map(c =>
            c.id === commentId
                ? { ...c, ...updatedRatings }
                : c
        ));
    };

    return (
        <div className="comments-section">
            <h2 className="comments-title">Guest Reviews</h2>

            {user && (
                <div className="comment-form-container">
                    <CommentForm
                        hotelId={hotelId}
                        onCommentSubmitted={handleCommentAdded}
                    />
                </div>
            )}

            {loading ? (
                <div className="loading-state">
                    <div className="spinner"></div>
                    <p>Loading comments...</p>
                </div>
            ) : error ? (
                <div className="error-state">
                    <p>❌ {error}</p>
                </div>
            ) : comments.length === 0 ? (
                <div className="empty-comments">
                    <p>No reviews yet. Be the first to share your experience!</p>
                </div>
            ) : (
                <div className="comments-list">
                    {comments.map(comment => (
                        <Comment
                            key={comment.id}
                            comment={comment}
                            hotelId={hotelId}
                            onDeleted={handleCommentDeleted}
                            onRatingUpdated={handleRatingUpdated}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default CommentsSection;
