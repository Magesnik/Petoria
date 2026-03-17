export const getBaseUrl = () => import.meta.env.VITE_API_URL || 'http://localhost:5150';
const API_BASE_URL = getBaseUrl() + '/api';

/**
 * Връща пълен URL за статичен ресурс (напр. качена аватарна снимка).
 * Обработва абсолютни URL-и (от Cloudinary и др.) и относителни пътища от бекенда.
 */
export const getAssetUrl = (path) => {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return (import.meta.env.VITE_API_URL || 'http://localhost:5150') + path;
};

/**
 * Обвивка около fetch, която автоматично включва credentials (бисквитки)
 * и обработва често срещани грешки.
 */
export const api = {
    get: (endpoint, options = {}) => request(endpoint, { ...options, method: 'GET' }),
    post: (endpoint, body, options = {}) => request(endpoint, { ...options, method: 'POST', body }),
    put: (endpoint, body, options = {}) => request(endpoint, { ...options, method: 'PUT', body }),
    delete: (endpoint, options = {}) => request(endpoint, { ...options, method: 'DELETE' }),
    // Връща пълния базов URL при нужда
    BASE_URL: API_BASE_URL
};

async function request(endpoint, options = {}) {
    // Проверява дали endpoint-ът е абсолютен или относителен
    const url = endpoint.startsWith('http') ? endpoint : `${API_BASE_URL}${endpoint}`;

    const defaultHeaders = {
        'Content-Type': 'application/json',
    };

    // Обединява хедърите
    const headers = {
        ...defaultHeaders,
        ...options.headers,
    };

    // Ако тялото е обект (и не е FormData), сериализира го в JSON
    let body = options.body;
    if (body && typeof body === 'object' && !(body instanceof FormData)) {
        body = JSON.stringify(body);
        // Задава content-type като JSON
        headers['Content-Type'] = 'application/json';
    } else if (body instanceof FormData) {
        // Оставя браузъра да зададе content-type за FormData (multipart/form-data)
        delete headers['Content-Type'];
    }

    const config = {
        ...options,
        headers,
        body,
        credentials: 'include', // Изпраща бисквитките с всяка заявка
    };

    try {
        const response = await fetch(url, config);

        // Обработва 401 Unauthorized глобално (напр. пренасочване към логин)
        // AuthContext се грижи за 401 при първоначално зареждане.

        // Проверява и парсва JSON отговора
        let data;
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            data = await response.json();
        } else {
            data = await response.text();
        }

        if (!response.ok) {
            // Обработва 401 чрез изпращане на custom event
            if (response.status === 401) {
                window.dispatchEvent(new CustomEvent('auth-unauthorized'));
            }

            // Създава обект за грешка със статус код
            const error = new Error(data.message || data || response.statusText || 'API request failed');
            error.status = response.status;
            throw error;
        }

        return data;
    } catch (error) {
        // Логва грешката само ако НЕ е 401 (Unauthorized)
        if (error.status !== 401) {
            console.error(`API Error (${options.method || 'GET'} ${endpoint}):`, error);
        }
        throw error;
    }
}
