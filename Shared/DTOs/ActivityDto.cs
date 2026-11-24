namespace Shared.DTOs
{
    /// <summary>
    /// DTO for reading activity data.
    /// </summary>
    public class ActivityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IncludesFood { get; set; }
        public bool AllowsGuest { get; set; }
        public decimal Cost { get; set; }
        public int? MaxParticipants { get; set; }
        public int? MinParticipants { get; set; }
        public string? ImageUrl { get; set; }
        public string? Requirements { get; set; }

        /// <summary>
        /// Number of registered participants: employees and visitors without an account ("gasten") together.
        /// A +1 is not a participant: it does not count towards the maximum.
        /// </summary>
        public int RegistrationCount { get; set; }

        /// <summary>
        /// Number of participants who bring a +1. Everyone can bring at most one, so this is also the number of +1's.
        /// </summary>
        public int PlusOneCount { get; set; }
    }
}
