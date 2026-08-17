using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services;

// Mirrors the API's SlotGridDto (spec 2.1). Deserialization matches by camelCase.
public class SlotGridResponse
{
    [JsonPropertyName("therapistId")] public Guid TherapistId { get; set; }
    [JsonPropertyName("weekStart")]   public DateTime WeekStart { get; set; }
    [JsonPropertyName("openHour")]    public int OpenHour { get; set; }
    [JsonPropertyName("closeHour")]   public int CloseHour { get; set; }
    [JsonPropertyName("cells")]       public List<SlotCellResponse> Cells { get; set; } = [];
}

public class SlotCellResponse
{
    [JsonPropertyName("scheduledAt")] public DateTime ScheduledAt { get; set; }
    [JsonPropertyName("dayIndex")]    public int DayIndex { get; set; }
    [JsonPropertyName("hour")]        public int Hour { get; set; }
    [JsonPropertyName("state")]       public string State { get; set; } = "Empty";
    [JsonPropertyName("slotId")]      public Guid? SlotId { get; set; }
}

// Mirrors the API's TherapistSlotOptionDto — the available slots offered in the
// "New Appointment" modal.
public class TherapistSlotResponse
{
    [JsonPropertyName("slotId")]      public Guid SlotId { get; set; }
    [JsonPropertyName("scheduledAt")] public DateTime ScheduledAt { get; set; }
}

// Mirrors the API's AppointmentRequestQueueDto (spec 4.1). Status arrives as a
// string (the API serializes enums as strings).
public class AppointmentRequestResponse
{
    [JsonPropertyName("appointmentId")] public Guid AppointmentId { get; set; }
    [JsonPropertyName("patientId")]     public Guid PatientId { get; set; }
    [JsonPropertyName("patientName")]   public string PatientName { get; set; } = string.Empty;
    [JsonPropertyName("therapistId")]   public Guid TherapistId { get; set; }
    [JsonPropertyName("therapistName")] public string TherapistName { get; set; } = string.Empty;
    [JsonPropertyName("scheduledAt")]   public DateTime ScheduledAt { get; set; }
    [JsonPropertyName("type")]          public string Type { get; set; } = string.Empty;
    [JsonPropertyName("notes")]         public string? Notes { get; set; }
    [JsonPropertyName("requestedAt")]   public DateTime RequestedAt { get; set; }
    [JsonPropertyName("status")]        public string Status { get; set; } = string.Empty;
}
