using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VB.Models.ViewModels;
using VB.Services;

namespace VB.Controllers
{
    [AllowAnonymous]
    public class SharedPasswordsController : Controller
    {
        private readonly IVaultShareService _vaultShareService;

        public SharedPasswordsController(IVaultShareService vaultShareService)
        {
            _vaultShareService = vaultShareService ?? throw new ArgumentNullException(nameof(vaultShareService));
        }

        [HttpGet]
        [Route("share/{token}")]
        [Route("SharedPasswords/Show/{token?}")]
        public async Task<IActionResult> Show(string token)
        {
            var result = await _vaultShareService.RetrieveAsync(token ?? string.Empty);

            SharedVaultViewModel viewModel = result.Status switch
            {
                ShareRetrievalStatus.Success when result.Payload != null => result.Payload,
                ShareRetrievalStatus.Expired => BuildError("Link expired", "This secure link has expired. Ask the owner to generate a new one."),
                ShareRetrievalStatus.Revoked => BuildError("Link revoked", "The owner revoked this share. It can no longer be viewed."),
                ShareRetrievalStatus.ViewLimitReached => BuildError("Link already used", "This secure link has reached its view limit."),
                _ => BuildError("Link unavailable", "We couldn’t find this secure share. It may be invalid or expired.")
            };

            return View("Show", viewModel);
        }

        private static SharedVaultViewModel BuildError(string title, string message) => new()
        {
            Success = false,
            Title = title,
            Message = message,
            RemainingViews = 0
        };
    }
}
