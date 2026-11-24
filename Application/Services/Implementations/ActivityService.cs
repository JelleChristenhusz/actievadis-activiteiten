using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.Exceptions;
using Application.Security;
using Shared.DTOs;
using Shared.Validation;
using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services.Implementations
{
    /// <summary>
    /// Service implementation for Activity business logic.
    /// </summary>
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repository;
        private readonly AppDbContext _context;

        public ActivityService(IActivityRepository repository, AppDbContext context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _context = context;
        }

        public async Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync()
        {
            var activities = await _repository.GetAllAsync();
            return activities.Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityDto>> GetVisibleActivitiesAsync()
        {
            // Deleted activities are already left out by the database query filter.
            // Times are stored as entered (local time), so compare with local time.
            var now = DateTime.Now;
            var activities = await _repository.GetAllAsync();

            return activities
                .Where(activity => activity.EndTime >= now)
                .OrderBy(activity => activity.StartTime)
                .Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityDto>> GetAllActivitiesForAdministratorAsync()
        {
            // Deleted activities stay in the database, but nobody sees them: the query filter leaves them out.
            var activities = await _repository.GetAllAsync();

            return activities
                .OrderBy(activity => activity.StartTime)
                .Select(MapToDto);
        }

        public async Task<ActivityDto?> GetActivityByIdAsync(Guid id)
        {
            var activity = await _repository.GetByIdAsync(id);
            return activity == null ? null : MapToDto(activity);
        }

        public async Task<ActivityDto> CreateActivityAsync(ActivityInputDto createDto)
        {
            var activity = new Activity
            {
                Name = createDto.Name,
                Location = createDto.Location,
                Description = createDto.Description,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                IncludesFood = createDto.IncludesFood,
                AllowsGuest = createDto.AllowsGuest,
                Cost = createDto.Cost,
                MaxParticipants = createDto.MaxParticipants,
                MinParticipants = createDto.MinParticipants,
                ImageUrl = createDto.ImageUrl,
                Requirements = createDto.Requirements
            };

            await _repository.AddAsync(activity);
            await _repository.SaveChangesAsync();

            return MapToDto(activity);
        }

        public async Task<ActivityDto?> UpdateActivityAsync(Guid id, ActivityInputDto updateDto)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null)
                return null;

            if (string.IsNullOrWhiteSpace(updateDto.Name) || string.IsNullOrWhiteSpace(updateDto.Location) ||
                string.IsNullOrWhiteSpace(updateDto.Description) || updateDto.StartTime == default ||
                updateDto.EndTime <= updateDto.StartTime || updateDto.Cost < 0 ||
                updateDto.MinParticipants < 1 || updateDto.MaxParticipants < 1 || updateDto.MinParticipants > updateDto.MaxParticipants)
                throw new ApplicationValidationException("Controleer de activiteitgegevens.", new[] { "Vul geldige tijden, kosten en deelnemersaantallen in." });
            // A +1 does not count towards the maximum, so only the participants themselves matter here.
            if (updateDto.MaxParticipants.HasValue && updateDto.MaxParticipants.Value < activity.Registrations.Count)
                throw new ConflictException("Het maximum mag niet lager zijn dan het aantal deelnemers.");
            activity.Name = updateDto.Name;
            activity.Location = updateDto.Location;
            activity.Description = updateDto.Description;
            activity.StartTime = updateDto.StartTime;
            activity.EndTime = updateDto.EndTime;
            activity.IncludesFood = updateDto.IncludesFood;
            activity.AllowsGuest = updateDto.AllowsGuest;
            activity.Cost = updateDto.Cost;
            activity.MaxParticipants = updateDto.MaxParticipants;
            activity.MinParticipants = updateDto.MinParticipants;
            activity.ImageUrl = updateDto.ImageUrl;
            activity.Requirements = updateDto.Requirements;

            _repository.Update(activity);
            await _repository.SaveChangesAsync();

            return MapToDto(activity);
        }

        public async Task<bool> DeleteActivityAsync(Guid id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null)
                return false;

            // Soft delete: the activity stays in the database, but disappears for everyone, administrators included.
            activity.DeletedAt = DateTime.Now;
            _repository.Update(activity);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ActivityDto>> GetActivitiesByLocationAsync(string location)
        {
            var activities = await _repository.GetActivitiesByLocationAsync(location);
            return activities.Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityDto>> GetActivitiesWithFoodAsync()
        {
            var activities = await _repository.GetActivitiesWithFoodAsync();
            return activities.Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityDto>> GetActivitiesByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var activities = await _repository.GetActivitiesByCostRangeAsync(minPrice, maxPrice);
            return activities.Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityDto>> GetUpcomingActivitiesAsync()
        {
            var activities = await _repository.GetActivitiesAfterDateAsync(DateTime.UtcNow);
            return activities.OrderBy(a => a.StartTime).Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityDto>> GetActivitiesWithAvailableSpotsAsync()
        {
            var activities = await _repository.GetActivitiesWithAvailableSpotAsync();
            return activities.Select(MapToDto);
        }

        public Task<bool> IsRegisteredAsync(Guid id, Guid userId) =>
            _context.Registrations.AnyAsync(r => r.ActivityId == id && r.UserId == userId);

        public async Task<RegistrationDto> GetRegistrationAsync(Guid id, Guid userId)
        {
            var registration = await _context.Registrations.AsNoTracking().SingleOrDefaultAsync(r => r.ActivityId == id && r.UserId == userId);
            return new RegistrationDto { IsRegistered = registration is not null, BringsGuest = registration?.BringsGuest ?? false, Comment = registration?.Comment };
        }

        public async Task<IEnumerable<ActivityRegistrationDto>> GetActivityRegistrationsAsync(Guid activityId)
        {
            // Deleted users are normally left out by the query filter, but their past participation is kept,
            // so they are listed here (marked as deleted). Their future registrations no longer exist.
            // Visitors without an account have no user, hence the left join.
            return await (from registration in _context.Registrations.AsNoTracking().IgnoreQueryFilters()
                          join user in _context.Users.AsNoTracking() on registration.UserId equals (Guid?)user.Id into users
                          from user in users.DefaultIfEmpty()
                          where registration.ActivityId == activityId
                          orderby registration.RegisteredAt
                          select new ActivityRegistrationDto
                          {
                              ParticipantName = user != null ? user.Name : registration.VisitorName!,
                              ParticipantDeleted = user != null && user.DeletedAt != null,
                              IsVisitor = registration.UserId == null,
                              VisitorEmail = registration.VisitorEmail,
                              VisitorPhone = registration.VisitorPhone,
                              BringsGuest = registration.BringsGuest,
                              Comment = registration.Comment,
                              RegisteredAt = registration.RegisteredAt
                          }).ToListAsync();
        }

        public async Task<ActivityDto?> RegisterAsync(Guid id, Guid userId, bool bringsGuest = false, string? comment = null)
        {
            // SQLite acquires a write lock before checking capacity, including concurrent requests.
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var activity = await _repository.GetByIdAsync(id);
            if (activity is null) return null;
            if (await IsRegisteredAsync(id, userId))
                throw new ConflictException("Je bent al ingeschreven voor deze activiteit.");
            EnsureCanRegister(activity, bringsGuest);
            _context.Registrations.Add(new Registration { ActivityId = id, UserId = userId, BringsGuest = bringsGuest, Comment = NormalizeComment(comment), RegisteredAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return MapToDto(activity);
        }

        public async Task<VisitorRegistrationResultDto?> RegisterVisitorAsync(Guid id, VisitorRegistrationDto visitor)
        {
            var name = visitor.Name?.Trim() ?? string.Empty;
            var email = NormalizeEmail(visitor.Email);
            var phone = visitor.Phone?.Trim() ?? string.Empty;
            var errors = new List<string>();
            if (name.Length == 0) errors.Add("Vul je naam in.");
            if (email.Length == 0) errors.Add("Vul je e-mailadres in.");
            else if (!EmailFormat.IsValid(email)) errors.Add(EmailFormat.ErrorMessage);
            if (phone.Length == 0) errors.Add("Vul je telefoonnummer in.");
            if (errors.Count > 0)
                throw new ApplicationValidationException("Validatie mislukt.", errors);

            // Same lock as for employees, so visitors and employees cannot take the last spot at the same time.
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var activity = await _repository.GetByIdAsync(id);
            if (activity is null) return null;
            if (activity.Registrations.Any(r => r.VisitorEmail == email))
                throw new ConflictException("Met dit e-mailadres ben je al ingeschreven voor deze activiteit.");
            // Employees register with their account, so their registration stays linked to them.
            if (await _context.Users.AnyAsync(user => user.Email == email))
                throw new ConflictException("Dit e-mailadres hoort bij een account. Log in om je in te schrijven.");
            EnsureCanRegister(activity, visitor.BringsGuest);

            var code = CancellationCodes.Create();
            _context.Registrations.Add(new Registration
            {
                ActivityId = id,
                VisitorName = name,
                VisitorEmail = email,
                VisitorPhone = phone,
                BringsGuest = visitor.BringsGuest,
                Comment = NormalizeComment(visitor.Comment),
                CancellationCodeHash = CancellationCodes.Hash(code),
                RegisteredAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new VisitorRegistrationResultDto { Activity = MapToDto(activity), CancellationCode = code };
        }

        public async Task<ActivityDto?> UpdateRegistrationAsync(Guid id, Guid userId, bool bringsGuest, string? comment)
        {
            var activity = await _repository.GetByIdAsync(id);
            var registration = activity?.Registrations.SingleOrDefault(r => r.UserId == userId);
            if (activity is null || registration is null) return null;
            EnsureCanChange(activity, "je inschrijving kan niet meer worden aangepast");
            // Dropping a +1 is always allowed; adding one only when the activity allows guests.
            if (bringsGuest && !registration.BringsGuest && !activity.AllowsGuest)
                throw new ConflictException("Bij deze activiteit is een +1 niet toegestaan.");
            registration.BringsGuest = bringsGuest;
            registration.Comment = NormalizeComment(comment);
            await _repository.SaveChangesAsync();
            return MapToDto(activity);
        }

        public async Task<bool> UnregisterAsync(Guid id, Guid userId)
        {
            var registration = await _context.Registrations.Include(r => r.Activity).SingleOrDefaultAsync(r => r.ActivityId == id && r.UserId == userId);
            if (registration is null) return false;
            EnsureCanChange(registration.Activity!, "uitschrijven kan niet meer");
            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VisitorCancellationResultDto?> CancelVisitorRegistrationAsync(string email, string code)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (normalizedEmail.Length == 0 || string.IsNullOrWhiteSpace(code)) return null;

            // One e-mail address can be registered for several activities, each with its own code.
            var candidates = await _context.Registrations.Include(r => r.Activity)
                .Where(r => r.UserId == null && r.VisitorEmail == normalizedEmail)
                .ToListAsync();
            var registration = candidates.FirstOrDefault(r => CancellationCodes.Verify(code, r.CancellationCodeHash));
            if (registration is null) return null;

            var activity = registration.Activity!;
            EnsureCanChange(activity, "uitschrijven kan niet meer");
            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();
            return new VisitorCancellationResultDto
            {
                ActivityId = activity.Id,
                ActivityName = activity.Name,
                ActivityStartTime = activity.StartTime,
                VisitorName = registration.VisitorName ?? string.Empty
            };
        }

        /// <summary>
        /// The rules for every new registration, from an employee or a visitor without an account.
        /// A registration deadline belongs here as well.
        /// </summary>
        private static void EnsureCanRegister(Activity activity, bool bringsGuest)
        {
            if (activity.StartTime <= DateTime.Now)
                throw new ConflictException("Deze activiteit is al begonnen; inschrijven is gesloten.");
            if (bringsGuest && !activity.AllowsGuest)
                throw new ConflictException("Bij deze activiteit is een +1 niet toegestaan.");
            // The maximum only counts participants (employees and visitors): a +1 never takes a spot.
            if (activity.MaxParticipants.HasValue && activity.Registrations.Count >= activity.MaxParticipants.Value)
                throw new ConflictException("Deze activiteit is vol.");
        }

        /// <summary>
        /// The rule for changing or cancelling an existing registration, from an employee or a visitor.
        /// An unregistration deadline belongs here as well.
        /// </summary>
        private static void EnsureCanChange(Activity activity, string whatIsNoLongerPossible)
        {
            if (activity.StartTime <= DateTime.Now)
                throw new ConflictException($"Deze activiteit is al begonnen; {whatIsNoLongerPossible}.");
        }

        private static string? NormalizeComment(string? comment) =>
            string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        private static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

        public async Task<IEnumerable<ActivityDto>> GetActivitiesForUserAsync(Guid userId)
        {
            var activities = await _repository.GetActivitiesForUserAsync(userId);
            return activities.Select(MapToDto);
        }

        private static ActivityDto MapToDto(Activity activity)
        {
            return new ActivityDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Location = activity.Location,
                Description = activity.Description,
                StartTime = activity.StartTime,
                EndTime = activity.EndTime,
                IncludesFood = activity.IncludesFood,
                AllowsGuest = activity.AllowsGuest,
                Cost = activity.Cost,
                MaxParticipants = activity.MaxParticipants,
                MinParticipants = activity.MinParticipants,
                ImageUrl = activity.ImageUrl,
                Requirements = activity.Requirements,
                RegistrationCount = activity.Registrations.Count,
                PlusOneCount = activity.Registrations.Count(r => r.BringsGuest)
            };
        }
    }
}
