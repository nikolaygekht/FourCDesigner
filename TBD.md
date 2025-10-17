TBD:
* debug register user and make sure email goes out (now the error is "too many unsucceful login attempts")
* make sure that reset password and register use sends links, not just codes
* make sure that we don't use parameters when forward to login.html from redirecting method, let's use cookies and get-data-once approach to receive this information
* add registration page
  * in the registation page check email is not used dynamically via script (onchange + delay)
  * let's add sugar to validate password locally (rules must be got via rest from the server)
* add password reset page

