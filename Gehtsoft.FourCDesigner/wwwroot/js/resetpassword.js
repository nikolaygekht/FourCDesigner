/**
 * Reset Password page functionality
 */

(function() {
    'use strict';

    // Configuration
    const API_BASE_URL = '$(external-prefix)/api/user';
    const LOGIN_PAGE = '$(external-prefix)/login.html';

    const resetForm = document.getElementById('reset-password-form');
    const passwordInput = document.getElementById('password');
    const resetButton = document.getElementById('reset-button');
    const messageDiv = document.getElementById('message');
    const passwordError = document.getElementById('password-error');
    const passwordRulesDiv = document.getElementById('password-rules');

    let passwordRules = null;
    let email = null;
    let token = null;

    /**
     * Shows a message to the user.
     * @param {string} message - The message to display.
     * @param {string} type - The message type ('success' or 'error').
     */
    function showMessage(message, type) {
        messageDiv.textContent = message;
        messageDiv.classList.remove('d-none', 'alert-success', 'alert-danger');
        messageDiv.classList.add(type === 'success' ? 'alert-success' : 'alert-danger');
    }

    /**
     * Hides the message.
     */
    function hideMessage() {
        messageDiv.classList.add('d-none');
    }

    /**
     * Validates the password field.
     * @returns {boolean} True if valid, false otherwise.
     */
    function validatePasswordField() {
        const password = passwordInput.value;
        const result = FourCApp.validatePassword(password, passwordRules);

        if (!result.valid) {
            passwordInput.classList.add('is-invalid');
            passwordError.textContent = result.errors[0];
            return false;
        }

        passwordInput.classList.remove('is-invalid');
        passwordInput.classList.add('is-valid');
        return true;
    }

    /**
     * Updates the reset button enabled state.
     */
    function updateResetButton() {
        const passwordValid = passwordInput.classList.contains('is-valid');
        resetButton.disabled = !passwordValid;
    }

    /**
     * Displays password rules to the user.
     */
    function displayPasswordRules() {
        if (!passwordRules)
            return;

        passwordRulesDiv.textContent = FourCApp.formatPasswordRules(passwordRules);
    }

    /**
     * Loads password validation rules from the server.
     */
    async function loadPasswordRules() {
        try {
            const response = await fetch(`${API_BASE_URL}/password-rules`);

            if (!response.ok) {
                console.error('ResetPassword: Failed to load password rules');
                // Use default rules as fallback
                passwordRules = {
                    minimumLength: 8,
                    requireCapitalLetter: true,
                    requireSmallLetter: true,
                    requireDigit: true,
                    requireSpecialSymbol: false
                };
                displayPasswordRules();
                return;
            }

            passwordRules = await response.json();
            displayPasswordRules();
            console.log('ResetPassword: Password rules loaded');
        } catch (error) {
            console.error('ResetPassword: Error loading password rules:', error);
            // Use default rules as fallback
            passwordRules = {
                minimumLength: 8,
                requireCapitalLetter: true,
                requireSmallLetter: true,
                requireDigit: true,
                requireSpecialSymbol: false
            };
            displayPasswordRules();
        } finally {
            // Signal that form is fully initialized for testing
            // This must happen after password rules are loaded and displayed
            window.resetPasswordFormInitialized = true;
        }
    }

    /**
     * Retrieves email and token from cookies.
     */
    function getResetCredentials() {
        const resetEmail = FourCApp.getCookie('reset_email');
        const resetToken = FourCApp.getCookie('reset_token');

        if (resetEmail && resetToken) {
            // Delete cookies immediately after reading
            FourCApp.deleteCookie('reset_email');
            FourCApp.deleteCookie('reset_token');
        }

        return {
            email: resetEmail,
            token: resetToken
        };
    }

    /**
     * Checks for and displays messages from cookies.
     */
    function checkForMessages() {
        const messageType = FourCApp.getCookie('login_message_type');
        const messageKey = FourCApp.getCookie('login_message');

        if (messageKey && messageType) {
            const messages = {
                'invalid_token': 'Invalid or expired password reset token. Please request a new password reset.',
                'reset_failed': 'Password reset failed. Please try again or request a new reset link.'
            };

            const message = messages[messageKey] || 'An error occurred. Please try again.';
            showMessage(message, 'error');

            FourCApp.deleteCookie('login_message');
            FourCApp.deleteCookie('login_message_type');
        }
    }


    /**
     * Handles the reset password form submission.
     * @param {Event} event - The form submit event.
     */
    async function handleResetPassword(event) {
        event.preventDefault();
        hideMessage();

        const password = passwordInput.value;

        if (!validatePasswordField())
            return;

        if (!email || !token) {
            showMessage('Invalid reset link. Please request a new password reset.', 'error');
            return;
        }

        resetButton.disabled = true;
        resetButton.textContent = 'Resetting...';

        try {
            const response = await fetch(`${API_BASE_URL}/reset-password`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    email: email,
                    token: token,
                    password: password
                })
            });

            const data = await response.json();

            if (!response.ok) {
                if (data.errors && data.errors.length > 0) {
                    const errorMessages = data.errors.map(e => e.messages.join(', ')).join('; ');
                    showMessage(errorMessages, 'error');
                } else if (data.message) {
                    showMessage(data.message, 'error');
                } else {
                    showMessage('Password reset failed. Please try again.', 'error');
                }
                return;
            }

            if (data.success) {
                // Set success cookie for login page
                FourCApp.setCookie('login_message', 'password_reset_success', 300);
                FourCApp.setCookie('login_message_type', 'success', 300);

                // Redirect immediately to login page
                window.location.href = LOGIN_PAGE;
            } else {
                showMessage(data.message || 'Password reset failed', 'error');
            }
        } catch (error) {
            console.error('ResetPassword: Error during password reset:', error);
            showMessage('An error occurred. Please try again.', 'error');
        } finally {
            resetButton.disabled = false;
            resetButton.textContent = 'Reset Password';
            updateResetButton();
        }
    }

    // Attach event listeners
    if (resetForm)
        resetForm.addEventListener('submit', handleResetPassword);

    if (passwordInput) {
        passwordInput.addEventListener('blur', () => {
            validatePasswordField();
            updateResetButton();
        });
        passwordInput.addEventListener('input', () => {
            if (passwordInput.classList.contains('is-invalid') || passwordInput.classList.contains('is-valid')) {
                validatePasswordField();
                updateResetButton();
            }
        });
    }

    // Initialize
    const credentials = getResetCredentials();
    email = credentials.email;
    token = credentials.token;

    if (!email || !token) {
        showMessage('Invalid reset link. Please request a new password reset from the login page.', 'error');
        resetButton.disabled = true;
    }

    loadPasswordRules();
    checkForMessages();

    console.log('ResetPassword: Page initialized');

})();
