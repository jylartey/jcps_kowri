using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class ChandaTypeController : Controller
    {
        private readonly ChandafyDbContext _context;

        public ChandaTypeController(ChandafyDbContext context)
        {
            _context = context;
        }

        // GET: ChandaType
        public async Task<IActionResult> Index()
        {
            var chandaType = await _context.ChandaTypes.ToListAsync();
            return View(chandaType);
        }

        // GET: ChandaType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ChandaType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] ChandaType chandaType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chandaType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chandaType);
        }

        // GET: ChandaType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chandaType = await _context.ChandaTypes.FindAsync(id);
            if (chandaType == null)
            {
                return NotFound();
            }
            return View(chandaType);
        }

        // POST: ChandaType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] ChandaType chandaType)
        {
            if (id != chandaType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chandaType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChandaTypeExists(chandaType.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chandaType);
        }

        // GET: ChandaType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chandaType = await _context.ChandaTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chandaType == null)
            {
                return NotFound();
            }

            return View(chandaType);
        }

        // POST: ChandaType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chandaType = await _context.ChandaTypes.FindAsync(id);
            _context.ChandaTypes.Remove(chandaType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChandaTypeExists(int id)
        {
            return _context.ChandaTypes.Any(e => e.Id == id);
        }
    }
}
