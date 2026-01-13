import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import './Comment.css';

const CommentForm = ({ hotelId, parentCommentId = null, onCommentSubmitted, onCancel }) => {
    const [text, setText] = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState('');
    const { user } = useAuth();

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!text.trim()) {
            setError('Please enter a comment');
            return;
        }

        setSubmitting(true);
        setError('');

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('http://localhost:5150/api/comments', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    hotelId,
                    text: text.trim(),
                    parentCommentId
                })
            });

            if (response.ok) {
                const newComment = await response.json();
                setText('');
                if (onCommentSubmitted) {
                    onCommentSubmitted(newComment);
                }
                if (onCancel) {
                    onCancel();
                }
            } else {
                setError('Failed to post comment');
            }
        } catch (err) {
            console.error('Error posting comment:', err);
            setError('Error posting comment');
        } finally {
            setSubmitting(false);
        }
    };

    if (!user) {
        return (
            <div className="comment-form-login-prompt">
                <p>Please log in to leave a comment</p>
            </div>
        );
    }

    return (
        <form className="comment-form" onSubmit={handleSubmit}>
            <textarea
                className="comment-input"
                placeholder={parentCommentId ? "Write a reply..." : "Share your experience..."}
                value={text}
                onChange={(e) => setText(e.target.value)}
                rows={parentCommentId ? 3 : 4}
                disabled={submitting}
            />
            {error && <div className="comment-error">{error}</div>}
            <div className="comment-form-actions">
                {onCancel && (
                    <button
                        type="button"
                        className="btn-cancel"
                        onClick={onCancel}
                        disabled={submitting}
                    >
                        Cancel
                    </button>
                )}
                <button
                    type="submit"
                    className="btn-submit"
                    disabled={submitting || !text.trim()}
                >
                    {submitting ? 'Posting...' : (parentCommentId ? 'Reply' : 'Post Comment')}
                </button>
            </div>
        </form>
    );
};

export default CommentForm;
