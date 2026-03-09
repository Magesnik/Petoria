import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const ProtectedRoute = ({ children, requireAdmin, requireModerator }) => {
    const { user, loading, isAdmin } = useAuth();
    const location = useLocation();

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
            </div>
        );
    }

    if (!user) {
        // Redirect them to the /login page, but save the current location they were
        // trying to go to when they were redirected. This allows us to send them
        // along to that page after they login, which is a nicer user experience
        // than dropping them off on the home page.
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (requireAdmin && !isAdmin()) {
        // Redirect to home if they are not an admin
        return <Navigate to="/" replace />;
    }

    // Determine if the user has a Moderator role.
    // AuthContext currently doesn't export an isModerator method, so we will check roles directly
    const isUserModerator = user?.roles?.includes('Moderator');

    if (requireModerator && !isUserModerator && !isAdmin()) {
        // Redirect to home if they are not a moderator or admin
        return <Navigate to="/" replace />;
    }

    return children;
};

export default ProtectedRoute;
