using Application.DTOs;
using Application.Exceptions;
using Application.Security;
using Application.Services.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementations;

/// <summary>
/// Manages users while enforcing unique email addresses and administrator protection.
/// The role always follows from the job title: Facilitair is an administrator, every other job title a normal user.
/// </summary>
public sealed class UserService(IUserRepository users, IRoleRepository roles) : IUserService
{
    /// <inheritdoc />
    public async Task<IEnumerable<UserDto>> GetAllAsync() =>
        (await users.GetAllWithRoleAsync()).Select(MapUser);

    /// <inheritdoc />
    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await users.GetByIdWithRoleAsync(id);
        return user is null ? null : MapUser(user);
    }

    /// <inheritdoc />
    public IEnumerable<JobTitleDto> GetJobTitles() =>
        JobTitles.All.Select(title => new JobTitleDto { Name = title, IsAdministrator = JobTitles.IsAdministrator(title) });

    /// <inheritdoc />
    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        var jobTitle = Validate(dto.Name, dto.JobTitle);
        if (await users.AnyAsync(user => user.Email == email))
            throw new ConflictException("Er bestaat al een gebruiker met dit e-mailadres.");

        var role = await GetRoleForAsync(jobTitle);
        var user = new User
        {
            Name = dto.Name.Trim(),
            JobTitle = jobTitle,
            Email = email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            RoleId = role.Id,
            Role = role
        };

        await users.AddAsync(user);
        await SaveChangesAsync();
        return MapUser(user);
    }

    /// <inheritdoc />
    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, Guid currentUserId)
    {
        var user = await users.GetByIdWithRoleAsync(id);
        if (user is null)
            return null;

        var email = NormalizeEmail(dto.Email);
        var jobTitle = Validate(dto.Name, dto.JobTitle);
        if (await users.AnyAsync(other => other.Id != id && other.Email == email))
            throw new ConflictException("Er bestaat al een gebruiker met dit e-mailadres.");

        var role = await GetRoleForAsync(jobTitle);
        if (user.Role.Name == RoleNames.Administrator && role.Name != RoleNames.Administrator)
        {
            // Changing your own job title away from Facilitair would lock you out of this page halfway through.
            if (id == currentUserId)
                throw new ConflictException("Je kunt je eigen beheerrechten niet afnemen. Vraag een andere beheerder om je functie te wijzigen.");
            await EnsureNotLastAdministratorAsync(user);
        }

        user.Name = dto.Name.Trim();
        user.JobTitle = jobTitle;
        user.Email = email;
        user.RoleId = role.Id;
        user.Role = role;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.PasswordHash = PasswordHasher.Hash(dto.Password);

        await SaveChangesAsync();
        return MapUser(user);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, Guid currentUserId)
    {
        var user = await users.GetByIdWithRegistrationsAsync(id);
        if (user is null)
            return false;

        if (id == currentUserId)
            throw new ConflictException("Je kunt je eigen account niet verwijderen.");
        await EnsureNotLastAdministratorAsync(user);

        // Soft delete: the user stays in the database but can no longer log in and disappears from every list.
        // Registrations for activities that have not started yet are deleted, so those spots become free again.
        // Registrations for activities that already started stay as history.
        var now = DateTime.Now;
        users.RemoveRegistrations(user.Registrations.Where(registration => registration.Activity!.StartTime > now).ToList());
        user.DeletedAt = now;

        await SaveChangesAsync();
        return true;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>Checks the name and job title, and returns the job title spelled exactly as in the fixed list.</summary>
    private static string Validate(string name, string jobTitle)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name)) errors.Add("Naam is verplicht.");
        var knownJobTitle = JobTitles.Find(jobTitle);
        if (knownJobTitle is null) errors.Add("Kies een functie uit de lijst.");
        if (errors.Count > 0)
            throw new ApplicationValidationException("Validatie mislukt.", errors);
        return knownJobTitle!;
    }

    private async Task<Role> GetRoleForAsync(string jobTitle) =>
        await roles.GetByIdAsync(JobTitles.RoleIdFor(jobTitle))
            ?? throw new InvalidOperationException("The roles are missing from the database.");

    private async Task EnsureNotLastAdministratorAsync(User user)
    {
        if (user.Role.Name == RoleNames.Administrator &&
            await users.CountByRoleNameAsync(RoleNames.Administrator) <= 1)
        {
            throw new ConflictException("De laatste beheerder (Facilitair) kan niet worden verwijderd of een andere functie krijgen.");
        }
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await users.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            throw new ConflictException("De wijziging conflicteert met bestaande gebruikersgegevens.", exception);
        }
    }

    private static UserDto MapUser(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        JobTitle = user.JobTitle,
        Email = user.Email,
        Role = MapRole(user.Role)
    };

    private static RoleDto MapRole(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description
    };
}
