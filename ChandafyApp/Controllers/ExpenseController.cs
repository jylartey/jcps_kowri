using ChandafyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChandafyApp.Controllers
{
    [Authorize(Roles = "ItAdmin,Admin,Regional,Muhtamim")]
    public class ExpenseController : Controller
    {
        private readonly ChandafyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ExpenseController(ChandafyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Expense
        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expenses
                .Include(e => e.FiscalYear)
                .ToListAsync();
            return View(expenses);
        }

        // GET: Expense/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
            return View();
        }

        // POST: Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense, IFormFile receiptImage)
        {
            if (ModelState.IsValid)
            {
                if (receiptImage != null && receiptImage.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads/receipts");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receiptImage.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await receiptImage.CopyToAsync(stream);
                    }

                    expense.ExpenseReceiptImage = fileName;
                }

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
            return View(expense);
        }

        // GET: Expense/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
            return View(expense);
        }

        // POST: Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense, IFormFile receiptImage)
        {
            if (id != expense.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (receiptImage != null && receiptImage.Length > 0)
                    {
                        var uploads = Path.Combine(_env.WebRootPath, "uploads/receipts");
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receiptImage.FileName);
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await receiptImage.CopyToAsync(stream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(expense.ExpenseReceiptImage))
                        {
                            var oldFilePath = Path.Combine(uploads, expense.ExpenseReceiptImage);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        expense.ExpenseReceiptImage = fileName;
                    }

                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.FiscalYears = await _context.FiscalYears.ToListAsync();
            return View(expense);
        }

        // GET: Expense/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.FiscalYear)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null) return NotFound();

            return View(expense);
        }

        // POST: Expense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            // Delete receipt image if exists
            if (!string.IsNullOrEmpty(expense.ExpenseReceiptImage))
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/receipts");
                var filePath = Path.Combine(uploads, expense.ExpenseReceiptImage);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id) => _context.Expenses.Any(e => e.Id == id);
    }
}
