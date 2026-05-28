using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services;

public class CreateAppointmentRequest
{
    public required string Title { get; set; }
    public string? Notes { get; set; }
    public Guid PatientId { get; set; }
    public string? TherapistName { get; set; }
    public DateTime AppointmentTime { get; set; }
}

public class UpdateAppointmentRequest
{
    public required string Title { get; set; }
    public string? Notes { get; set; }
    public string? TherapistName { get; set; }
    public DateTime AppointmentTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AppointmentSummaryResponse
{
    [JsonPropertyName("appointmentId")]   public Guid AppointmentId { get; set; }
    [JsonPropertyName("title")]           public string Title { get; set; } = string.Empty;
    [JsonPropertyName("appointmentTime")] public DateTime AppointmentTime { get; set; }
    [JsonPropertyName("status")]          public string Status { get; set; } = string.Empty;
    [JsonPropertyName("notes")]           public string? Notes { get; set; }
    [JsonPropertyName("patientName")]     public string PatientName { get; set; } = string.Empty;
    [JsonPropertyName("therapistName")]   public string TherapistName { get; set; } = string.Empty;
    [JsonPropertyName("patientId")]       public Guid PatientId { get; set; }
}
