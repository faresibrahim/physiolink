using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.ViewModels
{
    public class AppointmentListViewModel
    {
        public List<AppointmentSummaryResponse> Appointments { get; set; } = [];
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? TherapistFilter { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime WeekStart { get; set; }
        public required string ViewMode { get; set; }
        public List<TherapistResponse> Therapists { get; set; } = [];
        public int TotalAppointmentCount { get; set; }
        public List<PatientResponse> Patients { get; set; } = [];
        public PaginationViewModel Pagination { get; set; } = new();
    }

    public class AppointmentHistoryViewModel
    {
        public List<AppointmentSummaryResponse> Appointments { get; set; } = [];
        public int TotalCount { get; set; }
        public string? StatusFilter { get; set; }
        public string? TherapistFilter { get; set; }
        public List<TherapistResponse> Therapists { get; set; } = [];
        public PaginationViewModel Pagination { get; set; } = new();
    }

    public class CreateAppointmentViewModel
    {
        public Guid PatientId { get; set; }
        public string? TherapistName { get; set; }
        public required string Title { get; set; }
        public string? Notes { get; set; }
        public DateTime AppointmentTime { get; set; }
        // The open slot the admin picked in the modal; drives the booking server-side.
        public Guid? SlotId { get; set; }
    }

    public class EditAppointmentViewModel
    {
        public Guid AppointmentId { get; set; }
        // PatientId intentionally omitted - the hidden patient picker submits as ""
        // which fails Guid binding and makes ModelState invalid. The API's update
        // endpoint doesn't need it - the appointment is keyed by AppointmentId.
        public string? TherapistName { get; set; }
        public required string Title { get; set; }
        public string? Notes { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
