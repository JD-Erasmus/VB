using System;

namespace VB.Models.ViewModels
{
    public class SharedVaultViewModel
    {
        public bool Success { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string? WebsiteName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Url { get; set; }
        public string? RecipientNote { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public int RemainingViews { get; set; }
    }
}
