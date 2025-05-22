using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CAFMSystem.API.Models
{
    /// <summary>
    /// User entity that extends IdentityUser for authentication
    /// This represents all users in the CAFM system (End Users, Technicians, Managers, etc.)
    /// </summary>
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Department { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// Tickets created by this user (for End Users)
        /// </summary>
        public virtual ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

        /// <summary>
        /// Tickets assigned to this user (for Technicians/Managers)
        /// </summary>
        public virtual ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

        // Computed property for display
        public string FullName => $"{FirstName} {LastName}";
    }
}
