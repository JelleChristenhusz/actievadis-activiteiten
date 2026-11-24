using System;
using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// Links a participant to an activity. The participant is either an employee with an account (<see cref="UserId"/>),
    /// or a visitor without an account who registered through the website (called "gast" in the app);
    /// for a visitor the Visitor fields are filled instead.
    /// </summary>
    public class Registration : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid ActivityId { get; set; }
        public Activity? Activity { get; set; }

        /// <summary>The employee who registered, or null for a visitor without an account.</summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Brings one +1. Not to be confused with a visitor: a +1 never registers and never gives any details.
        /// </summary>
        public bool BringsGuest { get; set; }

        public string? Comment { get; set; }
        public DateTime RegisteredAt { get; set; }

        /// <summary>Name of the visitor without an account; null for an employee.</summary>
        public string? VisitorName { get; set; }

        /// <summary>E-mail address of the visitor (lower case); null for an employee.</summary>
        public string? VisitorEmail { get; set; }

        /// <summary>Phone number of the visitor; null for an employee.</summary>
        public string? VisitorPhone { get; set; }

        /// <summary>
        /// Hash of the code a visitor needs, together with the e-mail address, to unregister.
        /// The code itself is never stored. Null for an employee.
        /// </summary>
        public string? CancellationCodeHash { get; set; }

        /// <summary>True for a visitor without an account (a "gast").</summary>
        public bool IsVisitor => UserId is null;
    }
}
