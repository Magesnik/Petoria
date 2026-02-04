import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import CommentForm from './CommentForm';
import './Comment.css';

const Comment = ({ comment, hotelId, onDeleted, onRatingUpdated }) => {
    const [showReplyForm, setShowReplyForm] = useState(false);
    const [replies, setReplies] = useState(comment.replies || []);
    const { user } = useAuth();

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'just now';
        if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
        if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
        if (diffDays < 30) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;

        return date.toLocaleDateString();
    };

    const handleLike = async () => {
        if (!user) return;

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5150/api/comments/${comment.id}/rate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ isLike: true })
            });

            if (response.ok) {
                const data = await response.json();
                onRatingUpdated(comment.id, data);
            }
        } catch (err) {
            console.error('Error rating comment:', err);
        }
    };

    const handleDislike = async () => {
        if (!user) return;

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5150/api/comments/${comment.id}/rate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ isLike: false })
            });

            if (response.ok) {
                const data = await response.json();
                onRatingUpdated(comment.id, data);
            }
        } catch (err) {
            console.error('Error rating comment:', err);
        }
    };

    const handleDelete = async () => {
        if (!window.confirm('Are you sure you want to delete this comment?')) {
            return;
        }

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:5150/api/comments/${comment.id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                onDeleted(comment.id);
            }
        } catch (err) {
            console.error('Error deleting comment:', err);
        }
    };

    const handleReplySubmitted = (newReply) => {
        setReplies([...replies, newReply]);
        setShowReplyForm(false);
    };

    const handleReplyDelete = (replyId) => {
        setReplies(replies.filter(r => r.id !== replyId));
    };

    const handleReplyRatingUpdated = (replyId, updatedRatings) => {
        setReplies(replies.map(r =>
            r.id === replyId
                ? { ...r, ...updatedRatings }
                : r
        ));
    };

    const isOwner = user && user.id === comment.userId;
    const netLikes = comment.likesCount - comment.dislikesCount;

    return (
        <div className="comment">
            <div className="comment-header">
                <div className="comment-author">
                    <div className="author-avatar">
                        {comment.avatarUrl ? (
                            <img
                                src={comment.avatarUrl.startsWith('http') ? comment.avatarUrl : `http://localhost:5150${comment.avatarUrl}`}
                                alt={comment.firstName}
                            />
                        ) : (
                            <span>👤</span>
                        )}
                    </div>
                    <div className="author-info">
                        <span className="author-name">
                            {comment.firstName} {comment.lastName}
                        </span>
                        <span className="comment-date">{formatDate(comment.createdAt)}</span>
                    </div>
                </div>
                {isOwner && (
                    <button className="btn-delete-comment" onClick={handleDelete} title="Delete comment">
                        🗑️
                    </button>
                )}
            </div>

            <div className="comment-body">
                <p>{comment.text}</p>
            </div>

            <div className="comment-actions">
                <button
                    className={`btn-vote ${comment.userRating === true ? 'active' : ''}`}
                    onClick={handleLike}
                    disabled={!user}
                >
                    👍 {comment.likesCount}
                </button>
                <button
                    className={`btn-vote ${comment.userRating === false ? 'active' : ''}`}
                    onClick={handleDislike}
                    disabled={!user}
                >
                    👎 {comment.dislikesCount}
                </button>
                {netLikes > 0 && (
                    <span className="net-likes">+{netLikes}</span>
                )}
                {user && !showReplyForm && (
                    <button className="btn-reply" onClick={() => setShowReplyForm(true)}>
                        💬 Reply
                    </button>
                )}
            </div>

            {showReplyForm && (
                <div className="reply-form-container">
                    <CommentForm
                        hotelId={hotelId}
                        parentCommentId={comment.id}
                        onCommentSubmitted={handleReplySubmitted}
                        onCancel={() => setShowReplyForm(false)}
                    />
                </div>
            )}

            {replies.length > 0 && (
                <div className="comment-replies">
                    {replies.map(reply => (
                        <Comment
                            key={reply.id}
                            comment={reply}
                            hotelId={hotelId}
                            onDeleted={handleReplyDelete}
                            onRatingUpdated={handleReplyRatingUpdated}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default Comment;
