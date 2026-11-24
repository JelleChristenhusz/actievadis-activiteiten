using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Activity entity.
    /// </summary>
    public class ActivityRepository : Repository<Activity, Guid>, IActivityRepository
    {
        public ActivityRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override async Task<Activity?> GetByIdAsync(Guid id) =>
            await _dbSet.Include(activity => activity.Registrations)
                .SingleOrDefaultAsync(activity => activity.Id == id);

        /// <inheritdoc />
        public override async Task<IEnumerable<Activity>> GetAllAsync() =>
            await _dbSet.AsNoTracking().Include(activity => activity.Registrations).ToListAsync();

        /// <inheritdoc />
        public override async Task<IEnumerable<Activity>> FindAsync(Expression<Func<Activity, bool>> predicate) =>
            await _dbSet.AsNoTracking().Include(activity => activity.Registrations)
                .Where(predicate).ToListAsync();

        /// <inheritdoc />
        public async Task<IEnumerable<Activity>> GetActivitiesAfterDateAsync(DateTime date)
        {
            return await FindAsync(a => a.StartTime > date);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Activity>> GetActivitiesWithFoodAsync()
        {
            return await FindAsync(a => a.IncludesFood);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Activity>> GetActivitiesByCostRangeAsync(decimal minCost, decimal maxCost)
        {
            return await FindAsync(a => a.Cost >= minCost && a.Cost <= maxCost);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Activity>> GetActivitiesWithAvailableSpotAsync()
        {
            // A +1 does not take a spot, so only the participants are counted.
            return await FindAsync(a => !a.MaxParticipants.HasValue || a.Registrations.Count < a.MaxParticipants);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Activity>> GetActivitiesByLocationAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return new List<Activity>();

            return await FindAsync(a => a.Location.Contains(location));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Activity>> GetActivitiesForUserAsync(Guid userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(activity => activity.Registrations)
                .Where(activity => activity.Registrations.Any(registration => registration.UserId == userId))
                .OrderBy(activity => activity.StartTime)
                .ToListAsync();
        }
    }
}
