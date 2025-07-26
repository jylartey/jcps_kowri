using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin")]
    public class PaymentMethodController : Controller
    {
        private readonly ChandafyDbContext _context;

        public PaymentMethodController(ChandafyDbContext context)
        {
            _context = context;
        }

        // GET: PaymentMethod
        public async Task<IActionResult> Index()
        {
            var paymentMethods = await _context.PaymentMethods.ToListAsync();
            return View(paymentMethods);
        }

        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentMethod paymentMethod)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentMethod);
                await _context.SaveChangesAsync();
                return Json(new { success = true });

            }
            return Json(new { success = false });

        }

        // GET: PaymentMethod/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod == null) return NotFound();

            return Json(paymentMethod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,PaymentMethod paymentMethod)
        {
            if (id != paymentMethod.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentMethod);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentMethodExists(paymentMethod.Id)) return NotFound();
                    throw;
                }
                
            }
            return Json(new { success = false });
        }

        // GET: PaymentMethod/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paymentMethod == null) return NotFound();

            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        

        private bool PaymentMethodExists(int id) => _context.PaymentMethods.Any(e => e.Id == id);
    }
}
