using CAFMSystem.API.Models;
using CAFMSystem.API.DTOs;
using System.Text.RegularExpressions;

namespace CAFMSystem.API.Services
{
    /// <summary>
    /// Service for AI-like keyword routing and auto-suggestions
    /// This service analyzes ticket content and routes to appropriate technicians
    /// </summary>
    public interface IKeywordRoutingService
    {
        TicketCategory DetermineCategory(string title, string description);
        List<string> ExtractKeywords(string title, string description);
        List<KeywordSuggestionDto> GetSuggestions(string input);
        string GetRoleForCategory(TicketCategory category);
    }

    public class KeywordRoutingService : IKeywordRoutingService
    {
        /// <summary>
        /// Dictionary of keywords mapped to categories and roles
        /// In a real system, this could be loaded from a database or ML model
        /// </summary>
        private readonly Dictionary<TicketCategory, CategoryInfo> _categoryKeywords = new()
        {
            {
                TicketCategory.Plumbing,
                new CategoryInfo
                {
                    Role = "Plumber",
                    Keywords = new[]
                    {
                        "water", "leak", "pipe", "drain", "faucet", "toilet", "sink", "plumbing",
                        "flooding", "burst", "clog", "blockage", "sewage", "tap", "valve",
                        "pressure", "flow", "drip", "overflow", "backup"
                    }
                }
            },
            {
                TicketCategory.Electrical,
                new CategoryInfo
                {
                    Role = "Electrician",
                    Keywords = new[]
                    {
                        "power", "electric", "electrical", "socket", "outlet", "switch", "light",
                        "lighting", "bulb", "wire", "wiring", "circuit", "breaker", "fuse",
                        "voltage", "current", "shock", "sparks", "blackout", "outage"
                    }
                }
            },
            {
                TicketCategory.Cleaning,
                new CategoryInfo
                {
                    Role = "Cleaner",
                    Keywords = new[]
                    {
                        "dirty", "clean", "cleaning", "garbage", "trash", "waste", "spill",
                        "stain", "mess", "dust", "vacuum", "mop", "sanitize", "disinfect",
                        "hygiene", "odor", "smell", "restroom", "bathroom", "janitor"
                    }
                }
            },
            {
                TicketCategory.AssetManagement,
                new CategoryInfo
                {
                    Role = "AssetManager",
                    Keywords = new[]
                    {
                        "broken", "damaged", "repair", "replace", "equipment", "asset", "furniture",
                        "desk", "chair", "table", "cabinet", "door", "window", "lock",
                        "maintenance", "service", "inspection", "warranty", "inventory"
                    }
                }
            },
            {
                TicketCategory.HVAC,
                new CategoryInfo
                {
                    Role = "AssetManager", // Could be specialized HVAC technician
                    Keywords = new[]
                    {
                        "hvac", "heating", "cooling", "temperature", "thermostat", "ac", "air conditioning",
                        "ventilation", "fan", "duct", "filter", "hot", "cold", "climate",
                        "humidity", "airflow", "compressor", "refrigerant"
                    }
                }
            },
            {
                TicketCategory.Security,
                new CategoryInfo
                {
                    Role = "AssetManager", // Could be specialized security personnel
                    Keywords = new[]
                    {
                        "security", "alarm", "camera", "access", "card", "badge", "lock",
                        "key", "entry", "exit", "surveillance", "monitor", "breach",
                        "unauthorized", "theft", "safety", "emergency"
                    }
                }
            },
            {
                TicketCategory.IT,
                new CategoryInfo
                {
                    Role = "AssetManager", // Could be specialized IT support
                    Keywords = new[]
                    {
                        "computer", "laptop", "monitor", "screen", "keyboard", "mouse",
                        "network", "internet", "wifi", "printer", "software", "system",
                        "server", "database", "email", "phone", "telephone"
                    }
                }
            }
        };

        public TicketCategory DetermineCategory(string title, string description)
        {
            var text = $"{title} {description}".ToLowerInvariant();
            var words = ExtractWords(text);

            var categoryScores = new Dictionary<TicketCategory, int>();

            // Score each category based on keyword matches
            foreach (var kvp in _categoryKeywords)
            {
                var category = kvp.Key;
                var keywords = kvp.Value.Keywords;

                var score = keywords.Count(keyword => words.Contains(keyword));
                if (score > 0)
                {
                    categoryScores[category] = score;
                }
            }

            // Return the category with the highest score, or General if no matches
            return categoryScores.Any() 
                ? categoryScores.OrderByDescending(x => x.Value).First().Key 
                : TicketCategory.General;
        }

        public List<string> ExtractKeywords(string title, string description)
        {
            var text = $"{title} {description}".ToLowerInvariant();
            var words = ExtractWords(text);

            var extractedKeywords = new List<string>();

            foreach (var categoryInfo in _categoryKeywords.Values)
            {
                extractedKeywords.AddRange(
                    categoryInfo.Keywords.Where(keyword => words.Contains(keyword))
                );
            }

            return extractedKeywords.Distinct().ToList();
        }

        public List<KeywordSuggestionDto> GetSuggestions(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
                return new List<KeywordSuggestionDto>();

            var inputLower = input.ToLowerInvariant();
            var suggestions = new List<KeywordSuggestionDto>();

            foreach (var kvp in _categoryKeywords)
            {
                var category = kvp.Key;
                var keywords = kvp.Value.Keywords;

                foreach (var keyword in keywords)
                {
                    var relevance = CalculateRelevance(inputLower, keyword);
                    if (relevance > 0)
                    {
                        suggestions.Add(new KeywordSuggestionDto
                        {
                            Keyword = keyword,
                            Category = category,
                            CategoryText = category.ToString(),
                            Relevance = relevance
                        });
                    }
                }
            }

            return suggestions
                .OrderByDescending(s => s.Relevance)
                .ThenBy(s => s.Keyword)
                .Take(10)
                .ToList();
        }

        public string GetRoleForCategory(TicketCategory category)
        {
            return _categoryKeywords.TryGetValue(category, out var info) 
                ? info.Role 
                : "AssetManager"; // Default role
        }

        private List<string> ExtractWords(string text)
        {
            // Remove special characters and split into words
            var cleanText = Regex.Replace(text, @"[^\w\s]", " ");
            return cleanText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                           .Where(word => word.Length > 2) // Ignore very short words
                           .ToList();
        }

        private int CalculateRelevance(string input, string keyword)
        {
            // Exact match
            if (keyword.Equals(input, StringComparison.OrdinalIgnoreCase))
                return 100;

            // Starts with input
            if (keyword.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                return 80;

            // Contains input
            if (keyword.Contains(input, StringComparison.OrdinalIgnoreCase))
                return 60;

            // Input contains keyword (for longer inputs)
            if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return 40;

            return 0;
        }

        private class CategoryInfo
        {
            public string Role { get; set; } = string.Empty;
            public string[] Keywords { get; set; } = Array.Empty<string>();
        }
    }
}
