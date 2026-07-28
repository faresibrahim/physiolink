using System.ComponentModel.DataAnnotations;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.ViewModels;

public class TherapistListViewModel
{
    public List<TherapistResponse> Therapists { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public PaginationViewModel Pagination { get; set; } = new();
}

public class TherapistDetailViewModel
{
    public TherapistResponse Therapist { get; set; } = null!;
    public List<PatientResponse> AssignedPatients { get; set; } = [];

    // All therapists — feeds the shared "New Patient" dialog's assignment dropdown.
    public List<TherapistResponse> Therapists { get; set; } = [];
}

public class TherapistFormViewModel
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

    [Required(ErrorMessage = "Specialty is required.")]
    [StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters.")]
    public string Specialty { get; set; } = string.Empty;

    public bool IsEdit { get; set; }
    public Guid? TherapistId { get; set; }
}