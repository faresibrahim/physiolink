namespace PhysioLink.Application.Exceptions
{
    /// <summary>
    /// Thrown when an operation would create a second *active* user with a
    /// username that already belongs to another active account.
    /// </summary>
    public class UsernameInUseException : Exception
    {
        public UsernameInUseException(string username)
            : base($"An active account already exists for '{username}'.")
        {
            Username = username;
        }

        public string Username { get; }
    }
}
