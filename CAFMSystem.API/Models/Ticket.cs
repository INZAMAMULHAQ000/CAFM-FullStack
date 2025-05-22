using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAFMSystem.API.Models
{
    /// <summary>
    /// Ticket entity representing a maintenance/service request in the CAFM system
    /// </summary>
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Priority level: Low = 1, Medium = 2, High = 3, Critical = 4
        /// </summary>
        public int Priority { get; set; } = 2; // Default to Medium

        /// <summary>
        /// Status: Open = 1, InProgress = 2, Completed = 3, Closed = 4, Cancelled = 5
        /// </summary>
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        /// <summary>
        /// Category based on AI keyword routing
        /// </summary>
        public TicketCategory Category { get; set; }

        /// <summary>
        /// Optional image attachment path
        /// </summary>
        [StringLength(500)]
        public string? ImagePath { get; set; }

        /// <summary>
        /// Keywords extracted from title and description for routing
        /// </summary>
        [StringLength(500)]
        public string? ExtractedKeywords { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AssignedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        // Foreign Keys
        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public string? AssignedToUserId { get; set; }

        // Navigation Properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey("AssignedToUserId")]
        public virtual User? AssignedToUser { get; set; }

        // Computed properties
        public string PriorityText => Priority switch
        {
            1 => "Low",
            2 => "Medium",
            3 => "High",
            4 => "Critical",
            _ => "Unknown"
        };

        public bool IsOverdue => Status != TicketStatus.Completed && 
                                Status != TicketStatus.Closed && 
                                CreatedAt.AddDays(GetSLADays()) < DateTime.UtcNow;

        private int GetSLADays() => Priority switch
        {
            4 => 1,  // Critical: 1 day
            3 => 3,  // High: 3 days
            2 => 7,  // Medium: 7 days
            1 => 14, // Low: 14 days
            _ => 7
        };
    }

    /// <summary>
    /// Ticket status enumeration
    /// </summary>
    public enum TicketStatus
    {
        Open = 1,
        InProgress = 2,
        Completed = 3,
        Closed = 4,
        Cancelled = 5
    }

    /// <summary>
    /// Ticket category based on AI keyword routing
    /// </summary>
    public enum TicketCategory
    {
        General = 0,
        Plumbing = 1,
        Electrical = 2,
        Cleaning = 3,
        AssetManagement = 4,
        HVAC = 5,
        Security = 6,
        IT = 7
    }
}
