using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VB.Data;
using VB.Models;
using VB.Helpers;
using System.Linq;
using VB.Services;

namespace VB.Controllers
{
    [Authorize]
    public class VaultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionHelper _encryptionHelper;
        private readonly PasswordService _passwordService;

        public VaultsController(ApplicationDbContext context, IEncryptionHelper encryptionHelper, PasswordService passwordService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _encryptionHelper = encryptionHelper ?? throw new ArgumentNullException(nameof(encryptionHelper));
            _passwordService = passwordService;
        }

        // GET: Vaults
        public async Task<IActionResult> Index()
        {
            var vaults = await _context.Vault.ToListAsync();
            return View(vaults);
        }

        // GET: Vaults/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var vault = await _context.Vault.FindAsync(id);
            if (vault == null)
            {
                return NotFound();
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
            if (ModelState.IsValid)
            {
                vault.Password = _encryptionHelper.EncryptString(vault.Password);
                _context.Add(vault);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Vault created successfully.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            return View(vault);
        }

        // GET: Vaults/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var vault = await _context.Vault.FindAsync(id);
            if (vault == null)
            {
                return NotFound();
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
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vault.Password = _encryptionHelper.EncryptString(vault.Password);
                    _context.Update(vault);
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Vault updated successfully.";
                    TempData["StatusType"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VaultExists(vault.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vault);
        }

        // GET: Vaults/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var vault = await _context.Vault.FindAsync(id);
            if (vault == null)
            {
                return NotFound();
            }

            return View(vault);
        }

        // POST: Vaults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vault = await _context.Vault.FindAsync(id);
            if (vault == null)
            {
                return NotFound();
            }

            _context.Vault.Remove(vault);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Vault deleted successfully.";
            TempData["StatusType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        private bool VaultExists(int id)
        {
            return _context.Vault.Any(e => e.Id == id);
        }
    }
}