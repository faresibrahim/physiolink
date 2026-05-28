using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Therapists;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services //should live in infrastructure because DbContext is directly called in the service
{
    public class AdminTherapistService : IAdminTherapistService
    {
        private readonly ICurrentClinicService _currentClinicService;
        private readonly PhysioLinkDbContext _dbContext;

        public AdminTherapistService (ICurrentClinicService currentClinicService, PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
            _currentClinicService= currentClinicService;
        }

        public async Task<PagedResult<TherapistDto>> GetAllAsync(int page, int pageSize)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId();
            Console.WriteLine($"ClinicId from service: {clinicId}");

            var totalCount = await _dbContext.Therapists.CountAsync();

            var result = await _dbContext.Therapists.AsNoTracking()
                .Select(ea => new TherapistDto
            {
                TherapistId = ea.TherapistId,
                FirstName = ea.FirstName,
                LastName = ea.LastName,
                Email = ea.Email,
                PhoneNumber = ea.PhoneNumber,
                Speciality = ea.Speciality,
                IsActive = ea.IsActive,
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize).ToListAsync();
            
            return new PagedResult<TherapistDto>
            {
                Items = result,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

        }

        public async Task<TherapistDto?> GetByIdAsync(Guid id)
        {
            var therapist = await _dbContext.Therapists.AsNoTracking()
                .FirstOrDefaultAsync(t=>t.TherapistId == id);
            if (therapist == null)
            {
                return null;
            }
            return new TherapistDto
            {
                TherapistId = therapist.TherapistId,
                FirstName = therapist.FirstName,
                LastName = therapist.LastName,
                Email = therapist.Email,
                PhoneNumber = therapist.PhoneNumber,
                Speciality = therapist.Speciality,
                IsActive = therapist.IsActive,
            };
        } 

        public async Task<TherapistDto> CreateAsync(CreateTherapistDto createTherapistDto)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            var therapist = new Therapist(
                clinicId,
                createTherapistDto.FirstName,
                createTherapistDto.LastName,
                createTherapistDto.Email,
                createTherapistDto.PhoneNumber,
                createTherapistDto.Speciality,
                isActive: true
            );

            _dbContext.Therapists.Add(therapist);
            await _dbContext.SaveChangesAsync();

            return new TherapistDto
            {
                FirstName = therapist.FirstName,
                LastName = therapist.LastName,
                Email = therapist.Email,
                PhoneNumber = therapist.PhoneNumber,
                Speciality = therapist.Speciality,
                IsActive = therapist.IsActive,
                TherapistId = therapist.TherapistId
            };
        }

        public async Task<TherapistDto?> UpdateAsync(Guid id, UpdateTherapistDto updateTherapistDto)
        {
            var therapist = await _dbContext.Therapists.SingleOrDefaultAsync(t=>t.TherapistId == id);
            if (therapist == null)
            {
                return null;
            }

            therapist.FirstName = updateTherapistDto.FirstName;
            therapist.LastName = updateTherapistDto.LastName;
            therapist.Email = updateTherapistDto.Email;
            therapist.Speciality = updateTherapistDto.Speciality;
            therapist.IsActive = updateTherapistDto.IsActive;
            therapist.PhoneNumber = updateTherapistDto.PhoneNumber;

            await _dbContext.SaveChangesAsync();

            return new TherapistDto
            {
                TherapistId = therapist.TherapistId,
                FirstName  = therapist.FirstName,
                LastName = therapist.LastName,
                Email = therapist.Email,
                PhoneNumber = therapist.PhoneNumber,
                IsActive = therapist.IsActive,
                Speciality = therapist.Speciality
            };
        }

        public async Task<bool> DeleteAsync (Guid id)
        {
            var therapist = await _dbContext.Therapists.FirstOrDefaultAsync(t => t.TherapistId == id);
            if (therapist == null)
            {
                return false;
            }
            therapist.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }



    }
}