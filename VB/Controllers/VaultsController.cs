using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Infrastructure.Alerts;
using VB.Data;
using VB.Helpers;
using VB.Models;
using VB.Services;
using VB.Models.ViewModels;

namespace VB.Controllers
{
    [Authorize]
    public class VaultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionHelper _encryptionHelper;
        private readonly PasswordService _passwordService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IVaultShareService _vaultShareService;

        public VaultsController(ApplicationDbContext context, IEncryptionHelper encryptionHelper, PasswordService passwordService, UserManager<IdentityUser> userManager, IVaultShareService vaultShareService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _encryptionHelper = encryptionHelper ?? throw new ArgumentNullException(nameof(encryptionHelper));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _vaultShareService = vaultShareService ?? throw new ArgumentNullException(nameof(vaultShareService));
        }

        // GET: Vaults
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var vaults = await _context.Vault
                .Where(v => v.UserId == userId)
                .ToListAsync();
            return View(vaults);
        }

        // GET: Vaults/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var vault = await _context.Vault
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (vault == null)
            {
                this.SwalError("Vault not found.");
                return RedirectToAction(nameof(Index));
            }

            vault.Password = _encryptionHelper.DecryptString(vault.Password);
            return View(vault);
        }

        // GET: Vaults/Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GeneratePassword()
        {
            var generatedPassword = _passwordService.GeneratePassword();
            return Json(new { password = generatedPassword });
        }

        // POST: Vaults/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,Email,Url,WebsiteName")] Vault vault)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            vault.UserId = userId;
            ModelState.Remove(nameof(Vault.UserId));

            if (!ModelState.IsValid)
            {
                this.SwalWarning("Fix validation errors");
                return View(vault);
            }

            vault.Password = _encryptionHelper.EncryptString(vault.Password);
            _context.Add(vault);
            await _context.SaveChangesAsync();
            this.SwalSuccess("Vault created successfully.");
            return RedirectToAction(nameof(Index));
        }

        // GET: Vaults/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var vault = await _context.Vault
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (vault == null)
            {
                this.SwalError("Vault not found.");
                return RedirectToAction(nameof(Index));
            }

            vault.Password = _encryptionHelper.DecryptString(vault.Password);
            return View(vault);
        }

        // POST: Vaults/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Password,Email,Url,WebsiteName")] Vault vault)
        {
            if (id != vault.Id)
            {
                this.SwalError("Vault not found.");
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var existingVault = await _context.Vault
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (existingVault == null)
            {
                this.SwalError("Vault not found.");
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove(nameof(Vault.UserId));

            if (!ModelState.IsValid)
            {
                this.SwalWarning("Fix validation errors");
                return View(vault);
            }

            try
            {
                existingVault.Username = vault.Username;
                existingVault.Password = _encryptionHelper.EncryptString(vault.Password);
                existingVault.Email = vault.Email;
                existingVault.Url = vault.Url;
                existingVault.WebsiteName = vault.WebsiteName;

                await _context.SaveChangesAsync();
                this.SwalSuccess("Vault updated successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VaultExists(vault.Id, userId))
                {
                    this.SwalError("Vault not found.");
                    return RedirectToAction(nameof(Index));
                }

                this.SwalError("Unable to update vault. Please try again.");
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Vaults/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var vault = await _context.Vault
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (vault == null)
            {
                this.SwalError("Vault not found.");
                return RedirectToAction(nameof(Index));
            }

            return View(vault);
        }

        // POST: Vaults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            var vault = await _context.Vault
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (vault == null)
            {
                this.SwalError("Vault not found.");
                return RedirectToAction(nameof(Index));
            }

            _context.Vault.Remove(vault);
            await _context.SaveChangesAsync();
            this.SwalSuccess("Vault deleted successfully.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShare([FromBody] CreateVaultShareRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            if (request is null)
            {
                return BadRequest(new { message = "Invalid request payload." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Please review the form fields.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var vault = await _context.Vault
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == request.VaultId && v.UserId == userId);

            if (vault == null)
            {
                return NotFound(new { message = "Vault not found." });
            }

            var (share, rawToken) = await _vaultShareService.CreateShareAsync(vault, request);
            var link = Url.Action("Show", "SharedPasswords", new { token = rawToken }, Request.Scheme) ?? string.Empty;

            return Ok(new
            {
                link,
                shareId = share.Id,
                expiresAt = share.ExpiresAt,
                maxViews = share.MaxViews
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeShare([FromBody] int shareId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Challenge();
            }

            if (shareId <= 0)
            {
                return BadRequest(new { message = "Invalid share identifier." });
            }

            var revoked = await _vaultShareService.RevokeAsync(shareId, userId);
            if (!revoked)
            {
                return NotFound(new { message = "Share not found." });
            }

            return Ok(new { success = true });
        }

        private bool VaultExists(int id, string userId)
        {
            return _context.Vault.Any(e => e.Id == id && e.UserId == userId);
        }

        private string? GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}
