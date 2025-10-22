/**
 * Registration page functionality
 */

(function() {
    'use strict';

    // Configuration
    const API_BASE_URL = '/api/user';
    const LOGIN_PAGE = '/login.html';

    const registerForm = document.getElementById('register-form');
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const registerButton = document.getElementById('register-button');
    const messageDiv = document.getElementById('message');
    const emailError = document.getElementById('email-error');
    const passwordError = document.getElementById('password-error');
    const passwordRulesDiv = document.getElementById('password-rules');

    let passwordRules = null;

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


    var lastCheckEmail = null;
    var lastCheckResult = false;

    /**
     * Checks email availability with the server.
     * @param {string} email - The email to check.
     * @returns {Promise<boolean>} True if available, false otherwise.
     */
    async function checkEmailAvailability(email) {
        if (lastCheckEmail !== null && lastCheckEmail === email) {
            return lastCheckResult;
        }
        
        try {
            lastCheckEmail = email;
            const response = await fetch(`${API_BASE_URL}/check-email?email=${encodeURIComponent(email)}`);

            if (response.status === 429) {
                console.warn('Register: Email check rate limit exceeded');
                lastCheckResult = false;
                return false; // Assume available if rate limited
            }

            if (!response.ok) {
                console.error('Register: Email check failed');
                lastCheckResult = false;
                return false; // Assume available on error
            }

            const data = await response.json();
            lastCheckResult = data.available;
            return data.available;
        } catch (error) {
            console.error('Register: Error checking email availability:', error);
            lastCheckResult = false;
            return false; // Assume available on error
        }
    }

    /**
     * Validates the email field.
     * @returns {Promise<boolean>} True if valid, false otherwise.
     */
    async function validateEmailField() {
        const email = emailInput.value.trim();
        
        emailInput.classList.remove('is-invalid', 'is-valid');
        
        if (!email) {
            emailInput.classList.add('is-invalid');
            emailError.textContent = 'Email is required';
            return false;
        }

        if (!FourCApp.isValidEmail(email)) {
            emailInput.classList.add('is-invalid');
            emailError.textContent = 'Invalid email format';
            return false;
        }

        // Check email availability with server
        const available = await checkEmailAvailability(email);
        
        if (!available) {
            emailInput.classList.add('is-invalid');
            emailError.textContent = 'Email is already used';
            return false;
        }

        emailInput.classList.remove('is-invalid');
        emailInput.classList.add('is-valid');
        return true;
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
     * Updates the register button enabled state.
     */
    function updateRegisterButton() {
        const emailValid = emailInput.classList.contains('is-valid');
        const passwordValid = passwordInput.classList.contains('is-valid');
        registerButton.disabled = !(emailValid && passwordValid);
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
                console.error('Register: Failed to load password rules');
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
            console.log('Register: Password rules loaded');
        } catch (error) {
            console.error('Register: Error loading password rules:', error);
            // Use default rules as fallback
            passwordRules = {
                minimumLength: 8,
                requireCapitalLetter: true,
                requireSmallLetter: true,
                requireDigit: true,
                requireSpecialSymbol: false
            };
            displayPasswordRules();
        }
    }

    /**
     * Handles the registration form submission.
     * @param {Event} event - The form submit event.
     */
    async function handleRegister(event) {
        event.preventDefault();
        hideMessage();

        const email = emailInput.value.trim();
        const password = passwordInput.value;

        const emailValid = await validateEmailField();
        const passwordValid = validatePasswordField();

        if (!emailValid || !passwordValid)
            return;

        registerButton.disabled = true;
        registerButton.textContent = 'Registering...';

        try {
            const response = await fetch(`${API_BASE_URL}/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    email: email,
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
                    showMessage('Registration failed. Please try again.', 'error');
                }
                return;
            }

            if (data.success) {
                showMessage('Registration complete. Please check your email to confirm your account.', 'success');

                registerForm.reset();
                emailInput.classList.remove('is-valid');
                passwordInput.classList.remove('is-valid');

                setTimeout(() => {
                    window.location.href = LOGIN_PAGE;
                }, 5000);
            } else {
                showMessage(data.message || 'Registration failed', 'error');
            }
        } catch (error) {
            console.error('Register: Error during registration:', error);
            showMessage('An error occurred. Please try again.', 'error');
        } finally {
            registerButton.disabled = false;
            registerButton.textContent = 'Register';
            updateRegisterButton();
        }
    }

    // Attach event listeners
    if (registerForm)
        registerForm.addEventListener('submit', handleRegister);

    if (emailInput) {
         mailInput.addEventListener('blur', async () => {
            await validateEmailField();
            updateRegisterButton();
        });
    }

    if (passwordInput) {
        passwordInput.addEventListener('blur', () => {
            validatePasswordField();
            updateRegisterButton();
        });
        passwordInput.addEventListener('input', () => {
            if (passwordInput.classList.contains('is-invalid') || passwordInput.classList.contains('is-valid')) {
                validatePasswordField();
                updateRegisterButton();
            }
        });
    }

    // Initialize
    loadPasswordRules();
    console.log('Register: Page initialized');
    // Signal that form is fully initialized for testing
    window.registerFormInitialized = true;
})();
