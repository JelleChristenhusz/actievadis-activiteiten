using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Activity entity with additional custom operations.
    /// </summary>
    public interface IActivityRepository : IRepository<Activity, Guid>
    {
        /// <summary>
        /// Gets activities scheduled after a given date.
        /// </summary>
        Task<IEnumerable<Activity>> GetActivitiesAfterDateAsync(DateTime date);

        /// <summary>
        /// Gets activities that include food.
        /// </summary>
        Task<IEnumerable<Activity>> GetActivitiesWithFoodAsync();

        /// <summary>
        /// Gets activities within a given cost range.
        /// </summary>
        Task<IEnumerable<Activity>> GetActivitiesByCostRangeAsync(decimal minCost, decimal maxCost);

        /// <summary>
        /// Gets activities with available spots (participants < max).
        /// </summary>
        Task<IEnumerable<Activity>> GetActivitiesWithAvailableSpotAsync();

        /// <summary>
        /// Gets activities by location.
        /// </summary>
        Task<IEnumerable<Activity>> GetActivitiesByLocationAsync(string location);

        /// <summary>
        /// Gets the activities a user is registered for.
        /// </summary>
        Task<IEnumerable<Activity>> GetActivitiesForUserAsync(Guid userId);
    }
}
