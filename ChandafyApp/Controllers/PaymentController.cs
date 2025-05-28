using System;
using System.Linq;
using System.Threading.Tasks;
using ChandafyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace ChandafyApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ChandafyDbContext _dbContext;

        public PaymentController(ChandafyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Dashboard: Payments, budget completion, Chanda stats
        public async Task<IActionResult> Index()
        {
            try
            {
                var oneYearAgo = DateTime.Now.AddYears(-1);
                var payments = await _dbContext.Payments
                    .Include(p => p.Member)
                    .Include(p => p.ChandaType)
                    .Where(p => p.PaymentDate >= oneYearAgo)
                    .ToListAsync();

                var totalPaid = payments.Any() ? payments.Sum(p => p.Amount) : 0;

                var totalBudget = await _dbContext.Budgets
                    .Where(b => b.Year == DateTime.Now.Year)
                    .SumAsync(b => (decimal?)b.Amount) ?? 0;

                var percentBudgetCompleted = totalBudget > 0 ? (totalPaid / totalBudget) * 100 : 0;

                var chandaStats = payments.Count > 0
                    ? payments
                        .GroupBy(p => p.ChandaType.Name)
                        .Select(g => new { ChandaType = g.Key, Total = g.Sum(p => p.Amount) })
                        .ToList<object>() // Explicitly cast to List<object> to resolve CS0173
                    : new List<object>();

                ViewBag.TotalPaid = totalPaid;
                ViewBag.TotalBudget = totalBudget;
                ViewBag.PercentBudgetCompleted = percentBudgetCompleted;
                ViewBag.ChandaStats = chandaStats;
                return View(payments);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                ViewBag.ErrorMessage = "Unable to load payment or budget data. Please try again later.";
                ViewBag.TotalPaid = 0;
                ViewBag.TotalBudget = 0;
                ViewBag.PercentBudgetCompleted = 0;
                ViewBag.ChandaStats = new List<object>();
                return View(ViewBag);
            }
        }

        // Chanda Calculator
        [HttpGet]
        public IActionResult Calculator()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculator(decimal income, decimal chandaRate)
        {
            var chanda = income * (chandaRate / 100);
            ViewBag.CalculatedChanda = chanda;
            return View();
        }

        // Input Budget
        [HttpGet]
        public IActionResult Budget()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Budget(Budget model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Budgets.Add(model);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // Contact Info
        public IActionResult Contacts()
        {
            // You can fetch from DB or config
            ViewBag.Collector = "collector@email.com, 0800-000-0000";
            ViewBag.Qaid = "qaid@email.com, 0800-111-1111";
            return View();
        }

        // Print Receipt
        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Member)
                .Include(p => p.ChandaType)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        // Print Statement
        [HttpGet]
        public IActionResult Statement()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Statement(DateTime from, DateTime to)
        {
            var payments = await _dbContext.Payments
                .Include(p => p.Member)
                .Include(p => p.ChandaType)
                .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
                .ToListAsync();
            return View("StatementResult", payments);
        }

        // Make Payment
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Members"] = _dbContext.Members.ToList();
            ViewData["ChandaTypes"] = _dbContext.ChandaTypes.ToList();
            ViewData["PaymentMethods"] = _dbContext.PaymentMethods.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.PaymentDate = DateTime.Now;
                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["Members"] = _dbContext.Members.ToList();
            ViewData["ChandaTypes"] = _dbContext.ChandaTypes.ToList();
            ViewData["PaymentMethods"] = _dbContext.PaymentMethods.ToList();
            return View(payment);
        }
    }
}
