namespace PhysioLink.AdminPanel.Services;

// Thrown by ApiClient when the access token is rejected (401) and the refresh
// token can no longer produce a new one — i.e. the session is genuinely gone.
// Caught by SessionExpiredExceptionFilter, which clears the session and bounces
// the user to the login page instead of silently rendering an empty view.
public class SessionExpiredException : Exception
{
    public SessionExpiredException() : base("The session has expired.") { }
}
