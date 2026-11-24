using Domain.Common;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface for CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    public interface IRepository<T, TKey> where T : class, IEntity<TKey>
    {
        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        Task<T?> GetByIdAsync(TKey id);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Finds entities matching the given predicate.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Removes an entity.
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Checks if any entity matches the given predicate.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    }
}
