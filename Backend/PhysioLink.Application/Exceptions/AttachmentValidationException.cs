namespace PhysioLink.Application.Exceptions
{
    /// <summary>
    /// Thrown when an uploaded attachment fails validation — too large, an empty
    /// file, or a disallowed content type. The API maps this to a 400 with the message.
    /// </summary>
    public class AttachmentValidationException : Exception
    {
        public AttachmentValidationException(string message) : base(message) { }
    }
}
