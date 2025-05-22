using System.ComponentModel.DataAnnotations;
using CAFMSystem.API.Models;

namespace CAFMSystem.API.DTOs
{
    /// <summary>
    /// DTO for creating a new ticket
    /// </summary>
    public class CreateTicketDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Location { get; set; } = string.Empty;

        [Range(1, 4)]
        public int Priority { get; set; } = 2; // Default to Medium

        /// <summary>
        /// Optional base64 encoded image
        /// </summary>
        public string? ImageBase64 { get; set; }

        /// <summary>
        /// Image file name if provided
        /// </summary>
        public string? ImageFileName { get; set; }
    }

    /// <summary>
    /// DTO for updating a ticket
    /// </summary>
    public class UpdateTicketDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(300)]
        public string? Location { get; set; }

        [Range(1, 4)]
        public int? Priority { get; set; }

        public TicketStatus? Status { get; set; }

        public string? AssignedToUserId { get; set; }
    }

    /// <summary>
    /// DTO for ticket response
    /// </summary>
    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string PriorityText { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public TicketCategory Category { get; set; }
        public string CategoryText { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string? ExtractedKeywords { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsOverdue { get; set; }

        // User information
        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public string CreatedByUserEmail { get; set; } = string.Empty;
        public string? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public string? AssignedToUserEmail { get; set; }
    }

    /// <summary>
    /// DTO for ticket list with pagination
    /// </summary>
    public class TicketListDto
    {
        public List<TicketDto> Tickets { get; set; } = new List<TicketDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// DTO for ticket filtering and searching
    /// </summary>
    public class TicketFilterDto
    {
        public string? Search { get; set; }
        public TicketStatus? Status { get; set; }
        public TicketCategory? Category { get; set; }
        public int? Priority { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// DTO for keyword suggestions
    /// </summary>
    public class KeywordSuggestionDto
    {
        public string Keyword { get; set; } = string.Empty;
        public TicketCategory Category { get; set; }
        public string CategoryText { get; set; } = string.Empty;
        public int Relevance { get; set; } // 1-100 relevance score
    }
}
