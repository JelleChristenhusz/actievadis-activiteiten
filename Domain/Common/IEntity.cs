namespace Domain.Common
{
    /// <summary>
    /// Base interface for all domain entities.
    /// </summary>
    /// <typeparam name="TKey">The entity key type; GUID for business entities and integer for roles.</typeparam>
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
