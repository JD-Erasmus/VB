using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VB.Data;
using VB.Helpers;
using VB.Models;
using VB.Models.ViewModels;

namespace VB.Services
{
    public class VaultShareService : IVaultShareService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly ApplicationDbContext _context;
        private readonly IEncryptionHelper _encryptionHelper;
        private readonly ILogger<VaultShareService> _logger;

        public VaultShareService(
            ApplicationDbContext context,
            IEncryptionHelper encryptionHelper,
            ILogger<VaultShareService> logger)
        {
            _context = context;
            _encryptionHelper = encryptionHelper;
            _logger = logger;
        }

        public async Task<(VaultShare Share, string RawToken)> CreateShareAsync(
            Vault vault,
            CreateVaultShareRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(vault);
            ArgumentNullException.ThrowIfNull(request);

            var rawToken = GenerateToken();
            var tokenHash = HashToken(rawToken);
            var decryptedPassword = _encryptionHelper.DecryptString(vault.Password);

            var payload = new SharePayload
            {
                WebsiteName = vault.WebsiteName,
                Username = vault.Username,
                Password = decryptedPassword,
                Email = vault.Email,
                Url = vault.Url
            };

            var serializedPayload = JsonSerializer.Serialize(payload, SerializerOptions);
            var encryptedPayload = _encryptionHelper.EncryptString(serializedPayload);

            var share = new VaultShare
            {
                OwnerUserId = vault.UserId,
                VaultId = vault.Id,
                TokenHash = tokenHash,
                EncryptedPayload = encryptedPayload,
                RecipientNote = request.RecipientNote,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(request.ExpiresInMinutes),
                MaxViews = request.MaxViews
            };

            _context.VaultShares.Add(share);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vault share {ShareId} created for vault {VaultId}", share.Id, share.VaultId);

            return (share, rawToken);
        }

        public async Task<ShareRetrievalResult> RetrieveAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ShareRetrievalResult(ShareRetrievalStatus.NotFound, null);
            }

            var tokenHash = HashToken(token);
            var share = await _context.VaultShares
                .AsTracking()
                .FirstOrDefaultAsync(vs => vs.TokenHash == tokenHash, cancellationToken);

            if (share is null)
            {
                return new ShareRetrievalResult(ShareRetrievalStatus.NotFound, null);
            }

            var now = DateTimeOffset.UtcNow;

            if (share.RevokedAt.HasValue)
            {
                return new ShareRetrievalResult(ShareRetrievalStatus.Revoked, null);
            }

            if (share.ExpiresAt.HasValue && share.ExpiresAt.Value < now)
            {
                return new ShareRetrievalResult(ShareRetrievalStatus.Expired, null);
            }

            if (share.ViewCount >= share.MaxViews)
            {
                return new ShareRetrievalResult(ShareRetrievalStatus.ViewLimitReached, null);
            }

            SharePayload? payload;
            try
            {
                var decrypted = _encryptionHelper.DecryptString(share.EncryptedPayload);
                payload = JsonSerializer.Deserialize<SharePayload>(decrypted, SerializerOptions);
            }
            catch (Exception ex) when (ex is FormatException or JsonException or CryptographicException)
            {
                _logger.LogError(ex, "Failed to decrypt shared payload for share {ShareId}", share.Id);
                return new ShareRetrievalResult(ShareRetrievalStatus.NotFound, null);
            }

            if (payload is null)
            {
                return new ShareRetrievalResult(ShareRetrievalStatus.NotFound, null);
            }

            share.ViewCount += 1;
            share.FirstViewedAt ??= now;

            await _context.SaveChangesAsync(cancellationToken);

            var remainingViews = Math.Max(share.MaxViews - share.ViewCount, 0);

            var viewModel = new SharedVaultViewModel
            {
                Success = true,
                Title = payload.WebsiteName ?? "Shared Vault Entry",
                Message = "This entry was shared securely. Copy the details below.",
                WebsiteName = payload.WebsiteName,
                Username = payload.Username,
                Password = payload.Password,
                Email = payload.Email,
                Url = payload.Url,
                RecipientNote = share.RecipientNote,
                ExpiresAt = share.ExpiresAt,
                RemainingViews = remainingViews
            };

            return new ShareRetrievalResult(ShareRetrievalStatus.Success, viewModel);
        }

        public async Task<bool> RevokeAsync(int shareId, string ownerUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return false;
            }

            var share = await _context.VaultShares
                .AsTracking()
                .FirstOrDefaultAsync(vs => vs.Id == shareId && vs.OwnerUserId == ownerUserId, cancellationToken);

            if (share is null)
            {
                return false;
            }

            if (share.RevokedAt.HasValue)
            {
                return true;
            }

            share.RevokedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vault share {ShareId} revoked by owner {OwnerId}", shareId, ownerUserId);
            return true;
        }

        private static string GenerateToken()
        {
            Span<byte> buffer = stackalloc byte[32];
            RandomNumberGenerator.Fill(buffer);
            return Convert.ToHexString(buffer);
        }

        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        private sealed record SharePayload
        {
            public string? WebsiteName { get; init; }
            public string? Username { get; init; }
            public string? Password { get; init; }
            public string? Email { get; init; }
            public string? Url { get; init; }
        }
    }
}
