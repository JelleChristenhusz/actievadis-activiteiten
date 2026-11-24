namespace Domain.Constants;

/// <summary>Defines the role names used in persistence and authorization.</summary>
public static class RoleNames
{
    public const string Administrator = "Administrator";
    public const string User = "User";
}

/// <summary>Defines the stable integer keys of the built-in roles.</summary>
public static class RoleIds
{
    public const int Administrator = 1;
    public const int User = 2;
}
