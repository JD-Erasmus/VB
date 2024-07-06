using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VB.Data;
using VB.Models;
using VB.Helpers;
using Microsoft.AspNetCore.Authorization;


namespace VB.Controllers
{
    [Authorize]
    public class VaultsController : Controller
    {
        private readonly ApplicationDbContext _context;
      

        public VaultsController(ApplicationDbContext context)
        {
            _context = context;
           
        }
     
        // GET: Vaults
        public async Task<IActionResult> Index()
        {
              return _context.Vault != null ? 
                          View(await _context.Vault.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Vault'  is null.");
        }

        // GET: Vaults/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vault == null)
            {
                return NotFound();
            }

            var vault = await _context.Vault
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vault == null)
            {
                return NotFound();
            }
            vault.Password = EncryptionHelper.DecryptString(vault.Password);
            return View(vault);
        }

        // GET: Vaults/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vaults/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Password,Email,Url,WebsiteName")] Vault vault)
        {
            if (ModelState.IsValid)
            {
                vault.Password = EncryptionHelper.EncryptString(vault.Password);

                _context.Add(vault);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vault);
        }

        // GET: Vaults/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Vault == null)
            {
                return NotFound();
            }

            var vault = await _context.Vault.FindAsync(id);
            if (vault == null)
            {
                return NotFound();
            }
         
            return View(vault);
        }

        // POST: Vaults/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    vault.Password = EncryptionHelper.EncryptString(vault.Password);
                    _context.Update(vault);
                    await _context.SaveChangesAsync();
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Vault == null)
            {
                return NotFound();
            }

            var vault = await _context.Vault
                .FirstOrDefaultAsync(m => m.Id == id);
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
            if (_context.Vault == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Vault'  is null.");
            }
            var vault = await _context.Vault.FindAsync(id);
            if (vault != null)
            {
                _context.Vault.Remove(vault);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VaultExists(int id)
        {
          return (_context.Vault?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
