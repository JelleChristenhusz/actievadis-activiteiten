using System;
using System.Collections.Generic;
using Domain.Common;

namespace Domain.Entities
{
    /// <summary>Represents an activity and its registrations.</summary>
    public class Activity : IEntity<Guid>
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

        // Optional fields per requirements
        public int? MaxParticipants { get; set; }
        public int? MinParticipants { get; set; }
        public string? ImageUrl { get; set; }
        public string? Requirements { get; set; }

        /// <summary>
        /// When an administrator deleted the activity; null while it is active.
        /// Deleted activities are kept so administrators can still look them up.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted => DeletedAt.HasValue;

        public List<Registration> Registrations { get; set; } = new();
    }
}
