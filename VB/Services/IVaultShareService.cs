using System.Threading;
using System.Threading.Tasks;
using VB.Models;
using VB.Models.ViewModels;

namespace VB.Services
{
    public enum ShareRetrievalStatus
    {
        Success,
        NotFound,
        Expired,
        Revoked,
        ViewLimitReached
    }

    public record ShareRetrievalResult(ShareRetrievalStatus Status, SharedVaultViewModel? Payload);

    public interface IVaultShareService
    {
        Task<(VaultShare Share, string RawToken)> CreateShareAsync(
            Vault vault,
            CreateVaultShareRequest request,
            CancellationToken cancellationToken = default);

        Task<ShareRetrievalResult> RetrieveAsync(string token, CancellationToken cancellationToken = default);

        Task<bool> RevokeAsync(int shareId, string ownerUserId, CancellationToken cancellationToken = default);
    }
}
