using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Appointments
{
    // Single source of truth for the honest, user-facing status wording. Given there
    // is no notification layer (Phase 0 gaps), the patient app must never imply a
    // pending request is confirmed (spec Phase 7 "Status honesty").
    public static class AppointmentStatusLabels
    {
        public static string ToLabel(this AppointmentStatus status) => status switch
        {
            AppointmentStatus.Requested => "Pending — awaiting confirmation",
            AppointmentStatus.Confirmed => "Confirmed",
            AppointmentStatus.Completed => "Completed",
            AppointmentStatus.Rejected => "Rejected",
            AppointmentStatus.Expired => "Expired",
            AppointmentStatus.CancelledByClinic => "Cancelled by clinic",
            _ => status.ToString()
        };
    }
}
