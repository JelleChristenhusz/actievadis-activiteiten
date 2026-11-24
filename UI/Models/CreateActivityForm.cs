using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    /// <summary>Contains UI-only validation for the activity creation form.</summary>
    public class CreateActivityForm : IValidatableObject
    {
        [Required(ErrorMessage = "Activiteitnaam is verplicht.")]
        [StringLength(100, MinimumLength = 3,
                   ErrorMessage = "Activiteitnaam moet minimaal 3 tekens bevatten.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Locatie is verplicht.")]
        public string Location { get; set; } = string.Empty;

        public bool IncludesFood { get; set; }
        public bool AllowsGuest { get; set; }

        [Required(ErrorMessage = "Omschrijving is verplicht.")]
        [StringLength(2000, MinimumLength = 10,
            ErrorMessage = "Omschrijving moet minimaal 10 tekens bevatten.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Begintijd is verplicht.")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Eindtijd is verplicht.")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [Range(0, 10000,
            ErrorMessage = "Kosten moeten tussen de €0 en €10000 liggen.")]
        public decimal Cost { get; set; }

        [Range(1, 100,
            ErrorMessage = "Het maximaal aantal deelnemers moet tussen de 1 en 100 liggen.")]
        public int MaxParticipants { get; set; }

        [Range(1, 100,
            ErrorMessage = "Het minimaal aantal deelnemers moet tussen de 1 en 100 liggen.")]
        public int MinParticipants { get; set; }

        // Null represents an omitted image URL.
        [Url(ErrorMessage = "Afbeelding moet een geldige URL zijn (bijv. https://...).")]
        [StringLength(500, ErrorMessage = "De afbeelding is verplicht en mag maximaal 500 tekens bevatten.")]
        public string? ImageUrl { get; set; }

        [StringLength(1000, ErrorMessage = "Benodigdheden mag maximaal 1000 tekens bevatten.")]
        public string Requirements { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime == default)
            {
                yield return new ValidationResult(
                    "Begintijd is verplicht.",
                    new[] { nameof(StartTime) });
            }

            if (EndTime == default)
            {
                yield return new ValidationResult(
                    "Eindtijd is verplicht.",
                    new[] { nameof(EndTime) });
            }

            if (StartTime != default && StartTime < DateTime.Now)
            {
                yield return new ValidationResult(
                    "De begintijd mag niet in het verleden liggen.",
                    new[] { nameof(StartTime) });
            }

            if (StartTime != default && EndTime != default && EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "De eindtijd moet na de begintijd liggen.",
                    new[] { nameof(EndTime) });
            }

            if (MinParticipants > MaxParticipants)
            {
                yield return new ValidationResult(
                    "Het minimaal aantal deelnemers mag niet groter zijn dan het maximaal aantal deelnemers.",
                    new[] { nameof(MinParticipants), nameof(MaxParticipants) });
            }
        }
    }
}
