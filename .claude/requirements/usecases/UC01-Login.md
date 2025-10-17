# UC01 Login

## Page content

When the user opens the application they automatically should be routed to login.

At the login the system prompts:

* Username or email address
* Password
  * password must be entered in a masked input field (e.g. characters like '*' must be displayed instead of actual password when user types in)

The following three commands must be available on the login page:
* Login
* Register
* Reset Password

## When user clicks Login button.

If there is no email address or username entered yet, the error message "Enter the username or email and password" must be shown.
If both are entered, the credentials are sent to the server for validation.

The server validates the username and password and returns one of the following outcomes:
  * OK: Confirmation that the user exists, active and password is correct
    * If user successfully authorize a session must be created. The life time of the session is 30 minutes from the moment of the last operation.
    * The response also must contain the session token.
  * FAIL: The error: the user doesn't exist, isn't active and or password isn't correct
  * WAIT: The error: too many attempts, please wait
    - The error code must be returned if more than 3 failed login attempts happen in 1 minute interval (should be configurable)
  * LOCKED: The error: account is locked due to too many attempts
    - The error code must be returned if more than 20 failed login attempts happen in 1 hour interval (should be configurable)

NOTE: Details shall NOT be provided to avoid brute force attack facilitation. Just simple true/false response must be returned.

NOTE: The login method should be protected from DoS style attack and brute force attack by throttling too many requests from one IP. In case of throttling, HTTP 429 error must be returned.

If the user has successfully authorized (OK response), the user must be forwarded to `UC03-Create 4C Map.md` use case.

If the user isn't successfully authorized (FAIL response), the error message "The user doesn't exist, not active or password isn't correct" must be shown and the user stays on the same page.

If the user exceeds the wait threshold (WAIT response), the error message "Too many login attempts. Please wait for 1 minute before trying again" must be shown and the user stays on the same page.

If the user exceeds the lock threshold and account is locked (LOCKED response), the error message "Account is locked due to too many login attempts. Please contact the administration to unlock the account" must be shown and the user stays on the same page.

Login attempts must be logged:
  * Succesful attempts must be logged with the username, IP address and date/time
  * Unsuccessful attempts must be logged with the username, IP address and date/time

## When user clicks Reset Password Button

* The `UC01A-Change Password.md` is activated
* The message "If the user exists, the email with instructions on how to change the password will be sent"

## When user clicks Register button.

The user is forwarded to `UC02-Registration.md` use case

