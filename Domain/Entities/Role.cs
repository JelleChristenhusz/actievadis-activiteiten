using Domain.Common;

namespace Domain.Entities;

/// <summary>Represents an authorization role with a stable integer key.</summary>
public class Role : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<User> Users { get; set; } = new();
}
