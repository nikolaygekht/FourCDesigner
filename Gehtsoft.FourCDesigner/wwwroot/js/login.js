/**
 * Login page functionality
 */

(function() {
    'use strict';

    // Configuration
    const SESSION_STORAGE_KEY = 'sessionId';
    const API_BASE_URL = '/api/user';
    const HOME_PAGE = '/index.html';

    const loginForm = document.getElementById('login-form');
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const loginButton = document.getElementById('login-button');
    const errorMessage = document.getElementById('error-message');

    /**
     * Saves the session ID to local storage.
     * @param {string} sessionId - The session ID to save.
     */
    function saveSession(sessionId) {
        localStorage.setItem(SESSION_STORAGE_KEY, sessionId);
        console.log('Login: Session saved to storage');
    }

    /**
     * Shows an error message to the user.
     * @param {string} message - The error message to display.
     */
    function showError(message) {
        errorMessage.textContent = message;
        errorMessage.classList.remove('d-none');
    }

    /**
     * Hides the error message.
     */
    function hideError() {
        errorMessage.classList.add('d-none');
    }

    /**
     * Redirects to the home page.
     */
    function redirectToHome() {
        console.log('Login: Redirecting to home page');
        window.location.href = HOME_PAGE;
    }

    /**
     * Handles the login form submission.
     * @param {Event} event - The form submit event.
     */
    async function handleLogin(event) {
        event.preventDefault();
        hideError();

        const email = emailInput.value.trim();
        const password = passwordInput.value;

        if (!email || !password) {
            showError('Please enter both email and password');
            return;
        }

        // Disable the form during submission
        loginButton.disabled = true;
        loginButton.textContent = 'Logging in...';

        try {
            const response = await fetch(`${API_BASE_URL}/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    email: email,
                    password: password
                })
            });

            if (!response.ok) {
                if (response.status === 401) {
                    showError('Invalid email or password');
                } else {
                    showError('Login failed. Please try again.');
                }
                return;
            }

            const data = await response.json();

            if (data.sessionId) {
                saveSession(data.sessionId);
                redirectToHome();
            } else {
                showError('Invalid response from server');
            }
        } catch (error) {
            console.error('Login: Error during login:', error);
            showError('An error occurred. Please try again.');
        } finally {
            // Re-enable the form
            loginButton.disabled = false;
            loginButton.textContent = 'Login';
        }
    }

    // Attach event listener to the login form
    if (loginForm)
        loginForm.addEventListener('submit', handleLogin);

    console.log('Login: Page initialized');

})();
