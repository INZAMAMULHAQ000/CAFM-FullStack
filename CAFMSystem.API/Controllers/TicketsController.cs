using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CAFMSystem.API.Services;
using CAFMSystem.API.DTOs;
using System.Security.Claims;

namespace CAFMSystem.API.Controllers
{
    /// <summary>
    /// Controller for ticket management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IKeywordRoutingService _keywordRoutingService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ITicketService ticketService,
            IKeywordRoutingService keywordRoutingService,
            ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _keywordRoutingService = keywordRoutingService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new ticket
        /// </summary>
        /// <param name="createTicketDto">Ticket creation details</param>
        /// <returns>Created ticket</returns>
        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var ticket = await _ticketService.CreateTicketAsync(createTicketDto, userId);
                
                _logger.LogInformation($"Ticket {ticket.Id} created by user {userId}");
                
                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, "An internal error occurred while creating the ticket.");
            }
        }

        /// <summary>
        /// Get a specific ticket by ID
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <returns>Ticket details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound();
                }

                // Check if user has permission to view this ticket
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                if (!CanUserAccessTicket(ticket, userId, userRoles))
                {
                    return Forbid();
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ticket {id}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Get tickets with filtering and pagination
        /// </summary>
        /// <param name="filter">Filter parameters</param>
        /// <returns>Paginated list of tickets</returns>
        [HttpGet]
        public async Task<ActionResult<TicketListDto>> GetTickets([FromQuery] TicketFilterDto filter)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var primaryRole = userRoles.FirstOrDefault();

                var tickets = await _ticketService.GetTicketsAsync(filter, primaryRole, userId);
                
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Get tickets assigned to current user
        /// </summary>
        /// <returns>List of assigned tickets</returns>
        [HttpGet("my-assigned")]
        public async Task<ActionResult<TicketListDto>> GetMyAssignedTickets([FromQuery] TicketFilterDto filter)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                filter.AssignedToUserId = userId;
                var tickets = await _ticketService.GetTicketsAsync(filter);
                
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assigned tickets");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Get tickets created by current user
        /// </summary>
        /// <returns>List of created tickets</returns>
        [HttpGet("my-created")]
        public async Task<ActionResult<TicketListDto>> GetMyCreatedTickets([FromQuery] TicketFilterDto filter)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                filter.CreatedByUserId = userId;
                var tickets = await _ticketService.GetTicketsAsync(filter);
                
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting created tickets");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Update a ticket
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <param name="updateTicketDto">Update details</param>
        /// <returns>Updated ticket</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<TicketDto>> UpdateTicket(int id, [FromBody] UpdateTicketDto updateTicketDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingTicket = await _ticketService.GetTicketByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound();
                }

                // Check permissions
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                if (!CanUserModifyTicket(existingTicket, userId, userRoles))
                {
                    return Forbid();
                }

                var updatedTicket = await _ticketService.UpdateTicketAsync(id, updateTicketDto);
                if (updatedTicket == null)
                {
                    return NotFound();
                }

                _logger.LogInformation($"Ticket {id} updated by user {userId}");
                
                return Ok(updatedTicket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating ticket {id}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Assign a ticket to a user
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <param name="assignedToUserId">User ID to assign to</param>
        /// <returns>Success result</returns>
        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Admin,AssetManager")]
        public async Task<ActionResult> AssignTicket(int id, [FromBody] string assignedToUserId)
        {
            try
            {
                var success = await _ticketService.AssignTicketAsync(id, assignedToUserId);
                if (!success)
                {
                    return NotFound();
                }

                _logger.LogInformation($"Ticket {id} assigned to user {assignedToUserId}");
                
                return Ok(new { message = "Ticket assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning ticket {id}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Delete a ticket
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,AssetManager")]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            try
            {
                var success = await _ticketService.DeleteTicketAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                _logger.LogInformation($"Ticket {id} deleted");
                
                return Ok(new { message = "Ticket deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting ticket {id}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Get keyword suggestions for auto-complete
        /// </summary>
        /// <param name="input">Input text</param>
        /// <returns>List of keyword suggestions</returns>
        [HttpGet("suggestions")]
        public ActionResult<List<KeywordSuggestionDto>> GetKeywordSuggestions([FromQuery] string input)
        {
            try
            {
                var suggestions = _keywordRoutingService.GetSuggestions(input);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keyword suggestions");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        private bool CanUserAccessTicket(TicketDto ticket, string? userId, List<string> userRoles)
        {
            // Admin and AssetManager can access all tickets
            if (userRoles.Contains("Admin") || userRoles.Contains("AssetManager"))
                return true;

            // Users can access tickets they created
            if (ticket.CreatedByUserId == userId)
                return true;

            // Users can access tickets assigned to them
            if (ticket.AssignedToUserId == userId)
                return true;

            // Technicians can access tickets in their category
            if (userRoles.Contains("Plumber") && ticket.Category == Models.TicketCategory.Plumbing)
                return true;

            if (userRoles.Contains("Electrician") && ticket.Category == Models.TicketCategory.Electrical)
                return true;

            if (userRoles.Contains("Cleaner") && ticket.Category == Models.TicketCategory.Cleaning)
                return true;

            return false;
        }

        private bool CanUserModifyTicket(TicketDto ticket, string? userId, List<string> userRoles)
        {
            // Admin and AssetManager can modify all tickets
            if (userRoles.Contains("Admin") || userRoles.Contains("AssetManager"))
                return true;

            // Users can modify tickets assigned to them (status updates)
            if (ticket.AssignedToUserId == userId)
                return true;

            // End users can only modify their own tickets if they're still open
            if (ticket.CreatedByUserId == userId && ticket.Status == Models.TicketStatus.Open)
                return true;

            return false;
        }
    }
}
