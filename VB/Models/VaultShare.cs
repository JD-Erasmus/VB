using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VB.Models
{
    public class VaultShare
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VaultId { get; set; }

        [Required]
        [StringLength(450)]
        public string OwnerUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public string EncryptedPayload { get; set; } = string.Empty;

        [StringLength(200)]
        public string? RecipientNote { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ExpiresAt { get; set; }

        [Range(1, 25)]
        public int MaxViews { get; set; } = 1;

        public int ViewCount { get; set; }

        public DateTimeOffset? FirstViewedAt { get; set; }

        public DateTimeOffset? RevokedAt { get; set; }

        [ForeignKey(nameof(VaultId))]
        public Vault Vault { get; set; } = null!;
    }
}
