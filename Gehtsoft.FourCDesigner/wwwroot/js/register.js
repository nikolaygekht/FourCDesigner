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
    let systemEmail = null;

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
     * Checks email availability with the server.
     * @param {string} email - The email to check.
     * @returns {Promise<boolean>} True if available, false otherwise.
     */
    async function checkEmailAvailability(email) {
        try {
            const response = await fetch(`${API_BASE_URL}/check-email?email=${encodeURIComponent(email)}`);

            if (response.status === 429) {
                console.warn('Register: Email check rate limit exceeded');
                return true; // Assume available if rate limited
            }

            if (!response.ok) {
                console.error('Register: Email check failed');
                return true; // Assume available on error
            }

            const data = await response.json();
            return data.available;
        } catch (error) {
            console.error('Register: Error checking email availability:', error);
            return true; // Assume available on error
        }
    }

    /**
     * Validates the email field.
     * @returns {Promise<boolean>} True if valid, false otherwise.
     */
    async function validateEmailField() {
        const email = emailInput.value.trim();

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
            emailError.textContent = 'Email is already registered';
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
                return;
            }

            passwordRules = await response.json();
            displayPasswordRules();
            console.log('Register: Password rules loaded');
        } catch (error) {
            console.error('Register: Error loading password rules:', error);
        }
    }

    /**
     * Loads system email address from the server.
     */
    async function loadSystemEmail() {
        try {
            const response = await fetch(`${API_BASE_URL}/system-email`);

            if (!response.ok) {
                console.error('Register: Failed to load system email');
                return;
            }

            const data = await response.json();
            systemEmail = data.emailFrom;
            console.log('Register: System email loaded');
        } catch (error) {
            console.error('Register: Error loading system email:', error);
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
                const emailFrom = systemEmail || 'no-reply@swiftly.bz';
                const successMessage = `Registration complete. Please confirm your email address via email we have just sent you from ${emailFrom} email address`;
                showMessage(successMessage, 'success');

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

    // Debounce timer for email validation
    let emailValidationTimer = null;

    /**
     * Debounced email validation function.
     */
    function debouncedEmailValidation() {
        clearTimeout(emailValidationTimer);
        emailValidationTimer = setTimeout(async () => {
            await validateEmailField();
            updateRegisterButton();
        }, 500);
    }

    // Attach event listeners
    if (registerForm)
        registerForm.addEventListener('submit', handleRegister);

    if (emailInput) {
        emailInput.addEventListener('blur', async () => {
            clearTimeout(emailValidationTimer);
            await validateEmailField();
            updateRegisterButton();
        });
        emailInput.addEventListener('input', () => {
            if (emailInput.classList.contains('is-invalid') || emailInput.classList.contains('is-valid')) {
                debouncedEmailValidation();
            }
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
    loadSystemEmail();

    console.log('Register: Page initialized');

})();
