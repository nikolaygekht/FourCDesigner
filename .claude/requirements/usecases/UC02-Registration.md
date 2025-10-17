# Registration

The registration can be accessed by the link/forward from the login form (see `UC01-Login.md`) or by a direct link, e.g. from the email.

## Page content

The registration prompts:

* The email
* The username
* The first name
* The last name
* The password
  * Password must be entered in a masked input field (e.g. characters like '*' must be displayed instead of actual password when user types in).
  * There must be an toggle to show password
* The yes/no choice to agree to receive the marketing communications (yes by default).

Two commands must exist
 * "Register"
 * "Go to Login"

 NOTE: The page can preload password rules from the controller and show them to the user under the password field.

## When user clicks Register button.

First all data is validated locally:

* Email, username, first name must not be empty
* Email and user name must not already exists in the system
* Email must be a correct email according to RFC 5322 3.4.1 (https://www.rfc-editor.org/rfc/rfc5322#section-3.4.1)
* The password must match password rules

If validation failed, the appropriate error message must be shown.

Then a controller method is invoked.

The controller validates the data using the same rules as described above. If data is invalid as "data invalid error" is returned and appropriate message is shown.

Then controller creates the user using persistent layer component. All the users are created active.

The successful code is returned and "User is created, now you can login" message is shown.

NOTE: All the methods of the registration controller should be protected from DoS style attack and brute force attack by throttling too many requests from one IP. In case of throttling, HTTP 429 error must be returned.

NOTE: Not more than 2 attempts of the registration must be allowed in 15 seconds from the same IP (this should be configurable). If the user exceeds the threshold, the "please wait" return code must be returned and "Too many attempts, please wait 15 seconds" message must be displayed.

All user registrations must be logged. Log username, IP and timestamp of the operation and result.

