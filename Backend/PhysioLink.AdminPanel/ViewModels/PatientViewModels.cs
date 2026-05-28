using System.ComponentModel.DataAnnotations;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.ViewModels;

public class PatientListViewModel
{
    public List<PatientResponse> Patients { get; set; } = [];
    public List<TherapistResponse> Therapists { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SearchQuery { get; set; }
    public Guid? SelectedTherapistId { get; set; }
    public PaginationViewModel Pagination { get; set; } = new();
}

public class PatientDetailViewModel
{
    public PatientDetailResponse Patient { get; set; } = null!;
    public List<TherapistResponse> Therapists { get; set; } = [];
    public List<ExerciseResponse> Exercises { get; set; } = [];
}

public class PatientFormViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(100, ErrorMessage = "Phone number cannot exceed 100 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Diagnosis is required.")]
    [StringLength(500, ErrorMessage = "Diagnosis cannot exceed 500 characters.")]
    public string Diagnosis { get; set; } = string.Empty;

    public Guid? PatientId { get; set; }
    public Guid? TherapistId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEdit { get; set; }
    public List<TherapistResponse> Therapists { get; set; } = [];
}

public class CreatePatientResultViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
