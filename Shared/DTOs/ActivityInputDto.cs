using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    /// <summary>
    /// Contains the common input fields for creating or updating an activity.
    /// </summary>
    public class ActivityInputDto : IValidatableObject
    {
        [Required(ErrorMessage = "Dit veld is verplicht.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Dit veld is verplicht.")]
        public string Location { get; set; } = string.Empty;
        [Required(ErrorMessage = "Dit veld is verplicht.")]
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IncludesFood { get; set; }
        public bool AllowsGuest { get; set; }
        [Range(0, 10000)]
        public decimal Cost { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaxParticipants { get; set; }
        [Range(1, int.MaxValue)]
        public int? MinParticipants { get; set; }
        [Url]
        public string? ImageUrl { get; set; }
        public string? Requirements { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (StartTime == default || EndTime <= StartTime)
                yield return new ValidationResult("De eindtijd moet na de begintijd liggen.", new[] { nameof(EndTime) });
            if (MinParticipants > MaxParticipants)
                yield return new ValidationResult("Het minimum mag niet groter zijn dan het maximum.", new[] { nameof(MaxParticipants) });
        }
    }
}
