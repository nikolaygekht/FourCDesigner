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
    const forgotPasswordButton = document.getElementById('forgot-password-button');
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
        errorMessage.classList.remove('alert-success');
        errorMessage.classList.add('alert-danger');
    }

    /**
     * Shows a success message to the user.
     * @param {string} message - The success message to display.
     */
    function showSuccess(message) {
        errorMessage.textContent = message;
        errorMessage.classList.remove('d-none');
        errorMessage.classList.remove('alert-danger');
        errorMessage.classList.add('alert-success');
    }

    /**
     * Hides the error message.
     */
    function hideError() {
        errorMessage.classList.add('d-none');
    }

    /**
     * Validates and enables/disables the forgot password button.
     */
    function updateForgotPasswordButton() {
        const email = emailInput.value.trim();
        const isValid = email && FourCApp.isValidEmail(email);
        forgotPasswordButton.disabled = !isValid;
    }

    /**
     * Checks for and displays messages from cookies.
     */
    function checkForMessages() {
        const messageType = FourCApp.getCookie('login_message_type');
        const messageKey = FourCApp.getCookie('login_message');

        if (messageKey && messageType) {
            const messages = {
                'account_activated': 'Account activated successfully! You can now log in.',
                'invalid_activation_token': 'Invalid or expired activation token. Please try again or contact support.',
                'activation_failed': 'Account activation failed. Please try again or contact support.',
                'invalid_token': 'Invalid or expired password reset token. Please request a new password reset.',
                'password_reset_success': 'Password reset successfully! You can now log in with your new password.'
            };

            const message = messages[messageKey] || 'An error occurred. Please try again.';

            if (messageType === 'success')
                showSuccess(message);
            else
                showError(message);

            FourCApp.deleteCookie('login_message');
            FourCApp.deleteCookie('login_message_type');
        }
    }

    /**
     * Handles forgot password button click.
     */
    async function handleForgotPassword() {
        const email = emailInput.value.trim();

        if (!email || !FourCApp.isValidEmail(email)) {
            showError('Please enter a valid email address');
            return;
        }

        forgotPasswordButton.disabled = true;
        forgotPasswordButton.textContent = 'Sending...';
        hideError();

        try {
            const response = await fetch(`${API_BASE_URL}/request-password-reset`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    email: email
                })
            });

            if (response.ok) {
                showSuccess('If the user exists, an email with password reset instructions has been sent.');
                emailInput.value = '';
                passwordInput.value = '';
            } else {
                showError('Failed to process password reset request. Please try again.');
            }
        } catch (error) {
            console.error('Login: Error during password reset request:', error);
            showError('An error occurred. Please try again.');
        } finally {
            forgotPasswordButton.disabled = false;
            forgotPasswordButton.textContent = 'Forgot Password?';
            updateForgotPasswordButton();
        }
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

    // Attach event listener to forgot password button
    if (forgotPasswordButton)
        forgotPasswordButton.addEventListener('click', handleForgotPassword);

    // Update forgot password button state on email input
    if (emailInput) {
        emailInput.addEventListener('input', updateForgotPasswordButton);
        emailInput.addEventListener('blur', updateForgotPasswordButton);
    }

    // Check for messages from cookies on page load
    checkForMessages();

    console.log('Login: Page initialized');

})();
