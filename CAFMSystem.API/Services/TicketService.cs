using Microsoft.EntityFrameworkCore;
using CAFMSystem.API.Data;
using CAFMSystem.API.Models;
using CAFMSystem.API.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CAFMSystem.API.Services
{
    /// <summary>
    /// Service for handling ticket operations
    /// </summary>
    public interface ITicketService
    {
        Task<TicketDto> CreateTicketAsync(CreateTicketDto createTicketDto, string userId);
        Task<TicketDto?> GetTicketByIdAsync(int ticketId);
        Task<TicketListDto> GetTicketsAsync(TicketFilterDto filter, string? userRole = null, string? userId = null);
        Task<TicketDto?> UpdateTicketAsync(int ticketId, UpdateTicketDto updateTicketDto);
        Task<bool> DeleteTicketAsync(int ticketId);
        Task<bool> AssignTicketAsync(int ticketId, string assignedToUserId);
    }

    public class TicketService : ITicketService
    {
        private readonly CAFMDbContext _context;
        private readonly IKeywordRoutingService _keywordRoutingService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TicketService> _logger;
        private readonly IWebHostEnvironment _environment;

        public TicketService(
            CAFMDbContext context,
            IKeywordRoutingService keywordRoutingService,
            UserManager<User> userManager,
            ILogger<TicketService> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _keywordRoutingService = keywordRoutingService;
            _userManager = userManager;
            _logger = logger;
            _environment = environment;
        }

        public async Task<TicketDto> CreateTicketAsync(CreateTicketDto createTicketDto, string userId)
        {
            try
            {
                // Extract keywords and determine category using AI routing
                var extractedKeywords = _keywordRoutingService.ExtractKeywords(createTicketDto.Title, createTicketDto.Description);
                var category = _keywordRoutingService.DetermineCategory(createTicketDto.Title, createTicketDto.Description);

                // Handle image upload if provided
                string? imagePath = null;
                if (!string.IsNullOrEmpty(createTicketDto.ImageBase64) && !string.IsNullOrEmpty(createTicketDto.ImageFileName))
                {
                    imagePath = await SaveImageAsync(createTicketDto.ImageBase64, createTicketDto.ImageFileName);
                }

                // Create ticket
                var ticket = new Ticket
                {
                    Title = createTicketDto.Title,
                    Description = createTicketDto.Description,
                    Location = createTicketDto.Location,
                    Priority = createTicketDto.Priority,
                    Category = category,
                    ExtractedKeywords = string.Join(", ", extractedKeywords),
                    ImagePath = imagePath,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = TicketStatus.Open
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Auto-assign to appropriate technician if possible
                await AutoAssignTicketAsync(ticket);

                _logger.LogInformation($"Ticket {ticket.Id} created by user {userId} and categorized as {category}");

                return await MapToTicketDto(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                throw;
            }
        }

        public async Task<TicketDto?> GetTicketByIdAsync(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            return ticket != null ? await MapToTicketDto(ticket) : null;
        }

        public async Task<TicketListDto> GetTicketsAsync(TicketFilterDto filter, string? userRole = null, string? userId = null)
        {
            var query = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .AsQueryable();

            // Apply role-based filtering
            if (!string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(userId))
            {
                switch (userRole.ToLower())
                {
                    case "enduser":
                        // End users can only see their own tickets
                        query = query.Where(t => t.CreatedByUserId == userId);
                        break;
                    case "plumber":
                        // Plumbers see plumbing tickets assigned to them or unassigned plumbing tickets
                        query = query.Where(t => t.Category == TicketCategory.Plumbing && 
                                               (t.AssignedToUserId == userId || t.AssignedToUserId == null));
                        break;
                    case "electrician":
                        // Electricians see electrical tickets assigned to them or unassigned electrical tickets
                        query = query.Where(t => t.Category == TicketCategory.Electrical && 
                                               (t.AssignedToUserId == userId || t.AssignedToUserId == null));
                        break;
                    case "cleaner":
                        // Cleaners see cleaning tickets assigned to them or unassigned cleaning tickets
                        query = query.Where(t => t.Category == TicketCategory.Cleaning && 
                                               (t.AssignedToUserId == userId || t.AssignedToUserId == null));
                        break;
                    case "assetmanager":
                    case "admin":
                        // Asset managers and admins can see all tickets
                        break;
                }
            }

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(t => t.Title.Contains(filter.Search) || 
                                       t.Description.Contains(filter.Search) ||
                                       t.Location.Contains(filter.Search));
            }

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (filter.Category.HasValue)
                query = query.Where(t => t.Category == filter.Category.Value);

            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);

            if (!string.IsNullOrEmpty(filter.AssignedToUserId))
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);

            if (!string.IsNullOrEmpty(filter.CreatedByUserId))
                query = query.Where(t => t.CreatedByUserId == filter.CreatedByUserId);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.CreatedTo.Value);

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "title" => filter.SortDirection.ToLower() == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
                "priority" => filter.SortDirection.ToLower() == "asc" ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
                "status" => filter.SortDirection.ToLower() == "asc" ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status),
                "category" => filter.SortDirection.ToLower() == "asc" ? query.OrderBy(t => t.Category) : query.OrderByDescending(t => t.Category),
                _ => filter.SortDirection.ToLower() == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
            };

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tickets = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var ticketDtos = new List<TicketDto>();
            foreach (var ticket in tickets)
            {
                ticketDtos.Add(await MapToTicketDto(ticket));
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            return new TicketListDto
            {
                Tickets = ticketDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = filter.PageNumber > 1,
                HasNextPage = filter.PageNumber < totalPages
            };
        }

        public async Task<TicketDto?> UpdateTicketAsync(int ticketId, UpdateTicketDto updateTicketDto)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return null;

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateTicketDto.Title))
                ticket.Title = updateTicketDto.Title;

            if (!string.IsNullOrEmpty(updateTicketDto.Description))
                ticket.Description = updateTicketDto.Description;

            if (!string.IsNullOrEmpty(updateTicketDto.Location))
                ticket.Location = updateTicketDto.Location;

            if (updateTicketDto.Priority.HasValue)
                ticket.Priority = updateTicketDto.Priority.Value;

            if (updateTicketDto.Status.HasValue)
            {
                ticket.Status = updateTicketDto.Status.Value;
                
                // Update timestamps based on status
                switch (updateTicketDto.Status.Value)
                {
                    case TicketStatus.InProgress when ticket.AssignedAt == null:
                        ticket.AssignedAt = DateTime.UtcNow;
                        break;
                    case TicketStatus.Completed when ticket.CompletedAt == null:
                        ticket.CompletedAt = DateTime.UtcNow;
                        break;
                    case TicketStatus.Closed when ticket.ClosedAt == null:
                        ticket.ClosedAt = DateTime.UtcNow;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(updateTicketDto.AssignedToUserId))
            {
                ticket.AssignedToUserId = updateTicketDto.AssignedToUserId;
                ticket.AssignedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await MapToTicketDto(ticket);
        }

        public async Task<bool> DeleteTicketAsync(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignTicketAsync(int ticketId, string assignedToUserId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.AssignedToUserId = assignedToUserId;
            ticket.AssignedAt = DateTime.UtcNow;
            ticket.Status = TicketStatus.InProgress;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task AutoAssignTicketAsync(Ticket ticket)
        {
            try
            {
                var targetRole = _keywordRoutingService.GetRoleForCategory(ticket.Category);
                var availableUsers = await _userManager.GetUsersInRoleAsync(targetRole);
                
                // Simple round-robin assignment (in production, you might use more sophisticated logic)
                var activeUsers = availableUsers.Where(u => u.IsActive).ToList();
                if (activeUsers.Any())
                {
                    var assignedUser = activeUsers.OrderBy(u => Guid.NewGuid()).First(); // Random assignment
                    ticket.AssignedToUserId = assignedUser.Id;
                    ticket.AssignedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Ticket {ticket.Id} auto-assigned to {assignedUser.Email} ({targetRole})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to auto-assign ticket {ticket.Id}");
            }
        }

        private async Task<string> SaveImageAsync(string base64Image, string fileName)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "tickets");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                var imageBytes = Convert.FromBase64String(base64Image);
                await File.WriteAllBytesAsync(filePath, imageBytes);

                return $"/uploads/tickets/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                return null!;
            }
        }

        private async Task<TicketDto> MapToTicketDto(Ticket ticket)
        {
            // Ensure navigation properties are loaded
            if (ticket.CreatedByUser == null)
            {
                await _context.Entry(ticket)
                    .Reference(t => t.CreatedByUser)
                    .LoadAsync();
            }

            if (ticket.AssignedToUser == null && !string.IsNullOrEmpty(ticket.AssignedToUserId))
            {
                await _context.Entry(ticket)
                    .Reference(t => t.AssignedToUser)
                    .LoadAsync();
            }

            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Location = ticket.Location,
                Priority = ticket.Priority,
                PriorityText = ticket.PriorityText,
                Status = ticket.Status,
                StatusText = ticket.Status.ToString(),
                Category = ticket.Category,
                CategoryText = ticket.Category.ToString(),
                ImagePath = ticket.ImagePath,
                ExtractedKeywords = ticket.ExtractedKeywords,
                CreatedAt = ticket.CreatedAt,
                AssignedAt = ticket.AssignedAt,
                CompletedAt = ticket.CompletedAt,
                ClosedAt = ticket.ClosedAt,
                IsOverdue = ticket.IsOverdue,
                CreatedByUserId = ticket.CreatedByUserId,
                CreatedByUserName = ticket.CreatedByUser?.FullName ?? "Unknown",
                CreatedByUserEmail = ticket.CreatedByUser?.Email ?? "Unknown",
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToUserName = ticket.AssignedToUser?.FullName,
                AssignedToUserEmail = ticket.AssignedToUser?.Email
            };
        }
    }
}
