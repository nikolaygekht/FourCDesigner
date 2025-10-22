/**
 * Session Guard - Automatic session validation and management
 *
 * This script:
 * - Validates session on page load (except login.html)
 * - Periodically re-checks session every 60 seconds
 * - Redirects to login.html if session is invalid
 * - Manages session storage
 */

(function() {
    'use strict';

    // Configuration
    const SESSION_STORAGE_KEY = 'sessionId';
    const VALIDATION_INTERVAL = 60000; // 60 seconds
    const API_BASE_URL = '/api/user';
    const LOGIN_PAGE = '/login.html';

    // Skip session validation on public pages (login, register, reset password)
    const currentPage = window.location.pathname;
    const publicPages = ['login.html', 'login', 'register.html', 'register', 'resetpassword.html', 'resetpassword'];
    const isPublicPage = publicPages.some(page => currentPage.endsWith(page));

    if (isPublicPage) {
        console.log('Session guard: Skipping validation on public page:', currentPage);
        return;
    }

    /**
     * Gets the session ID from local storage.
     * @returns {string|null} The session ID or null if not found.
     */
    function getSessionId() {
        return localStorage.getItem(SESSION_STORAGE_KEY);
    }

    /**
     * Removes the session ID from local storage.
     */
    function clearSession() {
        localStorage.removeItem(SESSION_STORAGE_KEY);
        console.log('Session guard: Session cleared from storage');
    }

    /**
     * Redirects to the login page.
     */
    function redirectToLogin() {
        console.log('Session guard: Redirecting to login page');
        window.location.href = LOGIN_PAGE;
    }

    /**
     * Validates the session via API.
     * @param {string} sessionId - The session ID to validate.
     * @returns {Promise<boolean>} True if session is valid, false otherwise.
     */
    async function validateSession(sessionId) {
        if (!sessionId) {
            console.log('Session guard: No session ID found');
            return false;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/validate-session`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ sessionId: sessionId })
            });

            if (!response.ok) {
                console.warn('Session guard: Validation request failed with status', response.status);
                return false;
            }

            const data = await response.json();
            return data.valid === true;
        } catch (error) {
            console.error('Session guard: Error validating session:', error);
            return false;
        }
    }

    /**
     * Checks the session and redirects to login if invalid.
     */
    async function checkSession() {
        const sessionId = getSessionId();

        if (!sessionId) {
            console.log('Session guard: No session found, redirecting to login');
            redirectToLogin();
            return;
        }

        const isValid = await validateSession(sessionId);

        if (!isValid) {
            console.log('Session guard: Session is invalid, clearing and redirecting to login');
            clearSession();
            redirectToLogin();
        } else {
            console.log('Session guard: Session is valid');
        }
    }

    /**
     * Starts periodic session validation.
     */
    function startPeriodicValidation() {
        setInterval(async function() {
            console.log('Session guard: Performing periodic session check');
            await checkSession();
        }, VALIDATION_INTERVAL);
    }

    // Initialize session guard on page load
    console.log('Session guard: Initializing...');

    // Perform initial session check
    checkSession().then(function() {
        // Start periodic validation after initial check
        startPeriodicValidation();
        console.log('Session guard: Periodic validation started (every 60 seconds)');
    });

})();
