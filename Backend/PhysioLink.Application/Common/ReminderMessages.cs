namespace PhysioLink.Application.Common
{
    public record ReminderMessage(string Title, string Body);

    // All copy for the daily exercise reminder push lives here so it is easy to
    // edit. Keep it short, warm and free of medical claims.
    //
    // English only: the patient's language isn't stored server-side. An Arabic
    // version needs the patient's locale persisted (e.g. sent with the device token).
    public static class ReminderMessages
    {
        // Nothing logged yet today.
        public const string NotStartedTitle = "Time for your exercises";

        private static readonly string[] NotStartedBodies =
        {
            "A few minutes now keeps your recovery on track. You've got this!",
            "Your exercises are ready when you are. A short session today counts!",
            "Small steps every day add up. Let's do today's exercises!",
        };

        // Some, but not all, active exercises logged today.
        public const string PartlyDoneTitle = "Almost there";

        private static readonly string[] PartlyDoneBodies =
        {
            "You've done some of today's exercises. Ready to finish the rest?",
            "Nice work today! Just a few exercises left to finish.",
            "Great start today. A few more exercises and you're done!",
        };

        // Rotates the body by clinic-local day so the same patient doesn't get the
        // identical sentence every evening.
        public static ReminderMessage For(int doneToday, DateOnly clinicDay)
        {
            return doneToday == 0
                ? new ReminderMessage(NotStartedTitle, Pick(NotStartedBodies, clinicDay))
                : new ReminderMessage(PartlyDoneTitle, Pick(PartlyDoneBodies, clinicDay));
        }

        private static string Pick(string[] variants, DateOnly day) =>
            variants[day.DayOfYear % variants.Length];
    }
}
