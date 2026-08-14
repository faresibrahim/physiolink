namespace PhysioLink.Application.Exceptions
{
    /// <summary>
    /// Thrown when an operation would create a second *active* user with an
    /// email address that already belongs to another active account.
    /// </summary>
    public class EmailInUseException : Exception
    {
        public EmailInUseException(string email)
            : base($"An active account already exists for '{email}'.")
        {
            Email = email;
        }

        public string Email { get; }
    }
}
