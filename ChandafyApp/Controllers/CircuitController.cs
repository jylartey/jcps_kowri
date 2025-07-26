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

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Circuit circuit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(circuit);
                await _context.SaveChangesAsync();
                return Json(new { success = true });

            }
            return Json(new { success = false });

        }

        // GET: Circuit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var circuit = await _context.Circuits.FindAsync(id);
            if (circuit == null) return NotFound();

            return Json(circuit);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Circuit circuit)
        {
            if (id != circuit.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(circuit);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CircuitExists(circuit.Id)) return NotFound();
                    throw;
                }
                
            }
            return Json(new {success = false});
        }

        // GET: Circuit/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var circuit = await _context.Circuits
                .Include(c => c.Zone)
                .ThenInclude(z => z.Region)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (circuit == null) return NotFound();

            _context.Circuits.Remove(circuit);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

       

        private bool CircuitExists(int id) => _context.Circuits.Any(e => e.Id == id);
    }
}
