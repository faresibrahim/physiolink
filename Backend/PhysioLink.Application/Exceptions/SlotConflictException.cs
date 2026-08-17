namespace PhysioLink.Application.Exceptions
{
    // Thrown when a manual admin booking targets a slot that is no longer Available
    // (raced by a patient request or another admin). The API maps it to 409.
    public class SlotConflictException : Exception
    {
        public SlotConflictException(string message) : base(message) { }
    }
}
