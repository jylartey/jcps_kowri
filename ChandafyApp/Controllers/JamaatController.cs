using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class JamaatController : Controller
    {
        private readonly ChandafyDbContext _context;

        public JamaatController(ChandafyDbContext context)
        {
            _context = context;
        }

        // GET: Jamaat
        public async Task<IActionResult> Index()
        {
            var jamaats = await _context.Jamaats
                .Include(j => j.Circuit)
                .ThenInclude(c => c.Zone)
                .ThenInclude(z => z.Region)
                .ToListAsync();
            ViewBag.Circuits = await _context.Circuits.Include(c => c.Zone).ToListAsync();

            return View(jamaats);
        }

        // GET: Jamaat/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Circuits = await _context.Circuits.Include(c => c.Zone).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CircuitId")] Jamaat jamaat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jamaat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Circuits = await _context.Circuits.Include(c => c.Zone).ToListAsync();
            return View(jamaat);
        }

        // GET: Jamaat/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var jamaat = await _context.Jamaats.FindAsync(id);
            if (jamaat == null) return NotFound();

            ViewBag.Circuits = await _context.Circuits.Include(c => c.Zone).ToListAsync();
            return View(jamaat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CircuitId")] Jamaat jamaat)
        {
            if (id != jamaat.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jamaat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JamaatExists(jamaat.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Circuits = await _context.Circuits.Include(c => c.Zone).ToListAsync();
            return View(jamaat);
        }

        // GET: Jamaat/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var jamaat = await _context.Jamaats
                .Include(j => j.Circuit)
                .ThenInclude(c => c.Zone)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (jamaat == null) return NotFound();

            return View(jamaat);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jamaat = await _context.Jamaats.FindAsync(id);
            _context.Jamaats.Remove(jamaat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JamaatExists(int id) => _context.Jamaats.Any(e => e.Id == id);
    }
}
