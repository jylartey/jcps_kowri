using ChandafyApp.Data;
using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.Specialized;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class CircuitController : Controller
    {
        private readonly ChandafyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public class MemberViewModel
        {
            public decimal TotalProjectedAmount { get; set; }
            public double PercentageComplete { get; set; }
            public List<ApplicationUser> MembersYetToPay { get; set; }
        }


        public CircuitController(ChandafyDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET Circuit Collector
        public async Task<IActionResult> Collector() 
        {
            var user = await _userManager.GetUserAsync(User); // Get logged-in user's Identity ID

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Load the user with Jamaat and Circuit
            var userWithCircuit = await _context.Users
                .Include(u => u.Jamaat)
                .ThenInclude(j => j.Circuit)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userWithCircuit?.Jamaat?.CircuitId == null)
            {
                return NotFound("User's circuit not found.");
            }

            var collectorCircuitId = userWithCircuit.Jamaat.CircuitId;

            var totalCollected = await _context.Budgets
                .Where(b => _context.Users
                    .Any(u => u.Id == b.UserId && u.Jamaat.CircuitId == collectorCircuitId))
                .SumAsync(b => (decimal?)b.AmountPaid) ?? 0;

            var totalProjectedAmount = await _context.Budgets
                .Where(b => _context.Users
                    .Any(u => u.Id == b.UserId && u.Jamaat.CircuitId == collectorCircuitId))
                .SumAsync(b => (decimal?)b.TotalProjectedAmount) ?? 0;

            var activeFiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(fy => fy.IsActive == true);

            List<ApplicationUser> usersYetToPay = new();


            if (activeFiscalYear != null)
            {
                usersYetToPay = await _context.Users
                .Where(u => u.Jamaat.CircuitId == collectorCircuitId &&
                           !_context.Budgets
                               .Any(b => b.UserId == u.Id && b.FiscalYearId == activeFiscalYear.Id))
                .ToListAsync();
            }


            var viewModel = new MemberViewModel
            {
                TotalProjectedAmount = totalProjectedAmount,
                PercentageComplete = totalProjectedAmount > 0
                    ? (double)(totalCollected / totalProjectedAmount)
                    : 0,
                MembersYetToPay = usersYetToPay
            };

            return View(viewModel);

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
