import { useNavigate } from 'react-router-dom';

const API_BASE_URL = 'http://localhost:5150/api';

/**
 * Custom fetch wrapper that automatically includes credentials (cookies)
 * and handles common errors.
 */
export const api = {
    get: (endpoint, options = {}) => request(endpoint, { ...options, method: 'GET' }),
    post: (endpoint, body, options = {}) => request(endpoint, { ...options, method: 'POST', body }),
    put: (endpoint, body, options = {}) => request(endpoint, { ...options, method: 'PUT', body }),
    delete: (endpoint, options = {}) => request(endpoint, { ...options, method: 'DELETE' }),
    // Helper to get full URL if needed
    BASE_URL: API_BASE_URL
};

async function request(endpoint, options = {}) {
    // Ensure endpoint starts with / if not absolute
    const url = endpoint.startsWith('http') ? endpoint : `${API_BASE_URL}${endpoint}`;

    const defaultHeaders = {
        'Content-Type': 'application/json',
    };

    // Merge headers
    const headers = {
        ...defaultHeaders,
        ...options.headers,
    };

    // If body is an object (and not FormData), stringify it
    let body = options.body;
    if (body && typeof body === 'object' && !(body instanceof FormData)) {
        body = JSON.stringify(body);
        // Ensure content-type is json
        headers['Content-Type'] = 'application/json';
    } else if (body instanceof FormData) {
        // Let browser set content-type for FormData (multipart/form-data)
        delete headers['Content-Type'];
    }

    const config = {
        ...options,
        headers,
        body,
        credentials: 'include', // THIS IS KEY: Send cookies!
    };

    try {
        const response = await fetch(url, config);

        // Handle 401 Unauthorized globally if needed (e.g., redirect to login)
        // Note: We can't use useNavigate here directly as it's not a component.
        // AuthContext handles the 401 on initial load.

        // Parse JSON if possible
        let data;
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            data = await response.json();
        } else {
            data = await response.text();
        }

        if (!response.ok) {
            // Create error object with status
            const error = new Error(data.message || data || response.statusText || 'API request failed');
            error.status = response.status;
            throw error;
        }

        return data;
    } catch (error) {
        // Only log error if it's NOT a 401 (Unauthorized)
        if (error.status !== 401) {
            console.error(`API Error (${options.method || 'GET'} ${endpoint}):`, error);
        }
        throw error;
    }
}
