/**
 * Shared application utilities
 */

window.FourCApp = window.FourCApp || {};

(function() {
    'use strict';

    /**
     * Validates email format (RFC 2822 Section 3.4).
     * @param {string} email - The email to validate.
     * @returns {boolean} True if valid, false otherwise.
     */
    FourCApp.isValidEmail = function(email) {
        if (!email || typeof email !== 'string')
            return false;

        const pattern = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
        return pattern.test(email);
    };

    /**
     * Gets a cookie value by name.
     * @param {string} name - The cookie name.
     * @returns {string|null} The cookie value or null if not found.
     */
    FourCApp.getCookie = function(name) {
        const nameEQ = name + '=';
        const cookies = document.cookie.split(';');

        for (let i = 0; i < cookies.length; i++) {
            let cookie = cookies[i];
            while (cookie.charAt(0) === ' ')
                cookie = cookie.substring(1, cookie.length);

            if (cookie.indexOf(nameEQ) === 0)
                return cookie.substring(nameEQ.length, cookie.length);
        }
        return null;
    };

    /**
     * Deletes a cookie by name.
     * @param {string} name - The cookie name.
     */
    FourCApp.deleteCookie = function(name) {
        document.cookie = name + '=; Max-Age=0; path=/; SameSite=Strict';
    };

    /**
     * Sets a cookie.
     * @param {string} name - The cookie name.
     * @param {string} value - The cookie value.
     * @param {number} maxAgeSeconds - Max age in seconds (default 300 = 5 minutes).
     */
    FourCApp.setCookie = function(name, value, maxAgeSeconds) {
        maxAgeSeconds = maxAgeSeconds || 300;
        document.cookie = `${name}=${encodeURIComponent(value)}; Max-Age=${maxAgeSeconds}; path=/; SameSite=Strict`;
    };

    /**
     * Validates password against rules.
     * @param {string} password - The password to validate.
     * @param {Object} rules - Password rules object.
     * @returns {Object} Validation result with `valid` boolean and `errors` array.
     */
    FourCApp.validatePassword = function(password, rules) {
        const errors = [];

        if (!rules)
            return { valid: false, errors: ['Password rules not loaded'] };

        if (password.length < rules.minimumLength)
            errors.push(`Password must be at least ${rules.minimumLength} characters long`);

        if (rules.requireCapitalLetter && !/[A-Z]/.test(password))
            errors.push('Password must contain at least one uppercase letter');

        if (rules.requireSmallLetter && !/[a-z]/.test(password))
            errors.push('Password must contain at least one lowercase letter');

        if (rules.requireDigit && !/[0-9]/.test(password))
            errors.push('Password must contain at least one digit');

        if (rules.requireSpecialSymbol && !/[^a-zA-Z0-9]/.test(password))
            errors.push('Password must contain at least one special character');

        return {
            valid: errors.length === 0,
            errors: errors
        };
    };

    /**
     * Formats password rules for display.
     * @param {Object} rules - Password rules object.
     * @returns {string} Formatted rules string.
     */
    FourCApp.formatPasswordRules = function(rules) {
        if (!rules)
            return '';

        const rulesList = [];
        rulesList.push(`At least ${rules.minimumLength} characters`);

        if (rules.requireCapitalLetter)
            rulesList.push('One uppercase letter');

        if (rules.requireSmallLetter)
            rulesList.push('One lowercase letter');

        if (rules.requireDigit)
            rulesList.push('One digit');

        if (rules.requireSpecialSymbol)
            rulesList.push('One special character');

        return 'Password must contain: ' + rulesList.join(', ');
    };

    console.log('FourCApp: Shared utilities loaded');

})();
