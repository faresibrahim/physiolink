using PhysioLink.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace PhysioLink.Infrastructure.Services
{
    public class CurrentClinicService : ICurrentClinicService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentClinicService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid? GetCurrentClinicId()
        {
            var clinic = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "ClinicId")?.Value;
            if (Guid.TryParse(clinic, out var clinicId))
                return clinicId;
            return null;
        }
    }
}