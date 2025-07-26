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
        private readonly UserManager<IdentityUser> _userManager;

        public class MemberViewModel
        {
            public decimal TotalProjectedAmount { get; set; }
            public double PercentageComplete { get; set; }
            public List<Member> MembersYetToPay { get; set; }
        }


        public CircuitController(ChandafyDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET Circuit Collector
        public async Task<IActionResult> Collector() 
        {
            var userId = _userManager.GetUserId(User); // Get logged-in user's Identity ID

            // Get the Member associated with this user
            var member = await _context.Members
                .Include(m => m.Jamaat)
                .ThenInclude(j => j.Circuit)
                .FirstOrDefaultAsync(m => m.IdentityUserId == userId);

            if (member == null)
            {
                return NotFound("Member not found.");
            }

            var collectorCircuitId = member.Jamaat.CircuitId;
            var totalCollected = await _context.Budgets
                .Where(b => _context.Members
                .Any(m => m.Id == b.MemberId && m.Jamaat.CircuitId == collectorCircuitId))
                .SumAsync(b => (decimal?)b.AmountPaid) ?? 0;

            var totalProjectedAmount = await _context.Budgets
                .Where(b => _context.Members
                .Any(m => m.Id == b.MemberId && m.Jamaat.CircuitId == collectorCircuitId))
                .SumAsync(b => (decimal?)b.TotalProjectedAmount) ?? 0;

            var activeFiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(fy => fy.IsActive == true);
            
            List<Member> membersYetToPay = new();


            if (activeFiscalYear != null)
            {
                membersYetToPay = await _context.Members
                    .Where(m => !_context.Budgets
                        .Any(b => b.MemberId == m.Id && b.FiscalYearId == activeFiscalYear.Id))
                    .ToListAsync();
            }


            var viewModel = new MemberViewModel
            {
                TotalProjectedAmount = totalProjectedAmount,
                PercentageComplete = totalProjectedAmount > 0
                    ? (double)(totalCollected / totalProjectedAmount)
                    : 0,
                MembersYetToPay = membersYetToPay
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
