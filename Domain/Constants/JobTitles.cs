namespace Domain.Constants;

/// <summary>
/// The fixed job titles a user can have. The job title decides the role: Facilitair organises the activities
/// and is therefore an administrator; every other job title is a normal user. Nobody becomes an administrator any other way.
/// </summary>
public static class JobTitles
{
    public const string Meewerkstagiair = "Meewerkstagiair";
    public const string Afstudeerder = "Afstudeerder";
    public const string Ontwikkelaar1 = "Ontwikkelaar 1";
    public const string Ontwikkelaar2 = "Ontwikkelaar 2";
    public const string Teamlead = "Teamlead";
    public const string TechExpert = "Tech Expert";
    public const string HR = "HR";
    public const string Facilitair = "Facilitair";
    public const string Sales = "Sales";
    public const string Directeur = "Directeur";

    /// <summary>All job titles, in the order the user management form offers them.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Meewerkstagiair, Afstudeerder, Ontwikkelaar1, Ontwikkelaar2, Teamlead,
        TechExpert, HR, Facilitair, Sales, Directeur
    ];

    /// <summary>
    /// Returns the job title exactly as it is spelled in the list (ignoring case and surrounding spaces),
    /// or null when it is not one of the fixed job titles.
    /// </summary>
    public static string? Find(string? jobTitle) =>
        All.FirstOrDefault(title => string.Equals(title, jobTitle?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>True when users with this job title are administrators.</summary>
    public static bool IsAdministrator(string jobTitle) => jobTitle == Facilitair;

    /// <summary>The role that belongs to a job title: Facilitair is an administrator, every other job title a normal user.</summary>
    public static int RoleIdFor(string jobTitle) => IsAdministrator(jobTitle) ? RoleIds.Administrator : RoleIds.User;
}
