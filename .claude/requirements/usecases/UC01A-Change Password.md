# UC01A Change Password

This use cases starts not from the specific page, but from calling the controller method at Login form (see `UC01 Login.md`) after activating the "Reset Password" command.

At activation:

* The system checks whether the user exists
  * If the user doesn't exist then scenario ends, the control returns to the login form (we don't tell the user whether the username is not found to avoid guessing the username attack)
  * Otherwise the scenario continues

* One-time 6-digit code is generated and saved in the memory among with the username and creation timestamp
  * It must be a random code
  * Check whether there is no such active code at the moment

* An email text is created based on the template that accepts:
  * User name
  * URL to reset password
    * URL must contain the username and a one-time 6 digit code

* The email is sent to the user's email

At the moment the use cases pauses until the user receives, opens email and activates the link. The control flow gets back to the login form and appropriate message is shown there as defined in UC01.

When the user click on the URL in email a page which just activates another controller method should be opened:

* The 6-digit code must be validated
  * The code is valid if:
    - If the code exist
    - And it isn't expired yet (expiration of the code is always calculated from the moment when the code is created)
    - And username in URL matches to the username associated with the code
  * If code is valid, the user must be forwarded to the page to change the password
  * If code is invalid, the user must be forwarded to the error page

At the change password page the user must be prompted for the new password.

The form must contain just three controls:

* The new password
  * Password must be entered in a masked input field (e.g. characters like '*' must be displayed instead of actual password when user types in).
  * There must be an toggle to show password
* The command to reset the password
* The command to go to the login

NOTE: The page can preload password rules from the controller and show them to the user under the password field.

When user activates "reset the password" command

The password must be validated to the set of the rules defined in the configuration. These rules includes:
  * The minimum length of the password
  * Whether the password requires at least one uppercase letter
  * Whether the password requires at least one lowercase letter
  * Whether the password requires at least one digit
  * Whether the password requires at least one special character

The client side uses a controller method to get the password validation rules.

If password isn't valid, the error message is shown.

Otherwise, the controller method to change password is activated. The method must receive:

* The username
* The new password
* The code

It validates:
* The code. If code is invalid (see rules above), a error returned.
* The password. If password doesn't match the rules, a error code returned.

If code and password are valid, the controller changes the password for the user and returns successful code.

The change password page shows "Password succesfully changed" message. The user can go to login page using the "go to login" command.

The login controller interoperates with the following common services:

* Configuration to get the configuration
* Persistent layer to get users and validate the password
* Logging service to log information

Logging:
  * Each request to reset the password (log username, datetime and ip address)
  * Each attempt to reset the password (log username, datetime and ip address and error code)

