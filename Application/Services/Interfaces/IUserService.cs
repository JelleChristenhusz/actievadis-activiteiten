using Application.DTOs;

namespace Application.Services.Interfaces;

/// <summary>
/// Manages user profiles without exposing persistence entities.
/// The role follows from the job title: Facilitair is an administrator, every other job title a normal user.
/// </summary>
public interface IUserService
{
    /// <summary>Gets all users with their roles.</summary>
    Task<IEnumerable<UserDto>> GetAllAsync();
    /// <summary>Gets a user by GUID, or null when it does not exist.</summary>
    Task<UserDto?> GetByIdAsync(Guid id);
    /// <summary>Gets the fixed job titles a user can have, in the order a form should offer them.</summary>
    IEnumerable<JobTitleDto> GetJobTitles();
    /// <summary>Creates a user with a securely hashed password; the role follows from the job title.</summary>
    Task<UserDto> CreateAsync(CreateUserDto dto);
    /// <summary>
    /// Updates a user; the role follows from the job title. The last administrator keeps Facilitair,
    /// and the current user cannot take away their own administrator rights.
    /// </summary>
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, Guid currentUserId);
    /// <summary>
    /// Soft deletes a user: the user can no longer log in and disappears from every list, but stays in the database.
    /// Registrations for activities that have not started yet are removed; past ones are kept.
    /// The current user and the last administrator cannot be deleted. Returns false when the user does not exist.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, Guid currentUserId);
}
