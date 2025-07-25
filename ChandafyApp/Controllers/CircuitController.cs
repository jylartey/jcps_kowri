using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class CircuitController : Controller
    {
        private readonly ChandafyDbContext _context;

        public CircuitController(ChandafyDbContext context)
        {
            _context = context;
        }

        // GET: Circuit
        public async Task<IActionResult> Index()
        {
            var circuits = await _context.Circuits
                .Include(c => c.Zone)
                .ThenInclude(z => z.Region)
                .ToListAsync();
            ViewBag.Zones = await _context.Zones.Include(z => z.Region).ToListAsync();
            return View(circuits);
        }

        // GET: Circuit/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Zones = await _context.Zones.Include(z => z.Region).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ZoneId")] Circuit circuit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(circuit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Zones = await _context.Zones.Include(z => z.Region).ToListAsync();
            return View(circuit);
        }

        // GET: Circuit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var circuit = await _context.Circuits.FindAsync(id);
            if (circuit == null) return NotFound();

            ViewBag.Zones = await _context.Zones.Include(z => z.Region).ToListAsync();
            return View(circuit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ZoneId")] Circuit circuit)
        {
            if (id != circuit.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(circuit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CircuitExists(circuit.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Zones = await _context.Zones.Include(z => z.Region).ToListAsync();
            return View(circuit);
        }

        // GET: Circuit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var circuit = await _context.Circuits
                .Include(c => c.Zone)
                .ThenInclude(z => z.Region)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (circuit == null) return NotFound();

            return View(circuit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var circuit = await _context.Circuits.FindAsync(id);
            _context.Circuits.Remove(circuit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CircuitExists(int id) => _context.Circuits.Any(e => e.Id == id);
    }
}
