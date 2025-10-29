using System;
using System.ComponentModel.DataAnnotations;

namespace VB.Models.ViewModels
{
    public class CreateVaultShareRequest
    {
        [Required]
        public int VaultId { get; set; }

        [Range(1, 10080, ErrorMessage = "Expiry must be between 1 minute and 7 days.")]
        public int ExpiresInMinutes { get; set; } = 60;

        [Range(1, 25)]
        public int MaxViews { get; set; } = 1;

        [StringLength(200)]
        public string? RecipientNote { get; set; }
    }
}
