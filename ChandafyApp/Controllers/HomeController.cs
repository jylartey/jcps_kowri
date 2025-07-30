using ChandafyApp.Data;
using ChandafyApp.Models;
using ChandafyApp.NewFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using AccountSummary = ChandafyApp.NewFolder.AccountSummary;

namespace ChandafyApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ChandafyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ILogger<HomeController> logger, ChandafyDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var activeFiscalYear = await _context.GetActiveFiscalYearAsync();
            if (activeFiscalYear == null)
            {
                return View(new DashboardViewModel
                {
                    AccountSummary = new AccountSummary(),
                    FiscalYears = new List<FiscalYear>(),
                    RecentPayments = new List<Payment>(),
                    ChandaTypePayments = new Dictionary<string, decimal>(),
                    MonthlyBudget = new List<decimal>(new decimal[12]),
                    MonthlyPayments = new List<decimal>(new decimal[12])
                });
            }

            var currentUser = await _userManager.GetUserAsync(User);

            
           
            var fiscalYears = await _context.FiscalYears.OrderByDescending(f => f.Period).ToListAsync();
            var viewModel = new DashboardViewModel
            {
                CurrentFiscalYear = activeFiscalYear,
                FiscalYears = fiscalYears
            };
            if (User.IsInRole("Member"))
            {
                
                // Member-specific data
                if (currentUser != null)
                {
                    viewModel.AccountSummary = await GetAccountSummary(currentUser.Id, activeFiscalYear.Id);
                    viewModel.RecentPayments = await GetRecentPayments(currentUser.Id);
                    viewModel.ChandaTypePayments = await GetChandaTypePayments(currentUser.Id, activeFiscalYear.Id);
                    (viewModel.MonthlyBudget, viewModel.MonthlyPayments) = await GetMonthlyData(currentUser.Id, activeFiscalYear.Id);
                }
            }


            else if (User.IsInRole("Collector") || User.IsInRole("LocalAdmin"))
            {
                // Collector/LocalAdmin can see their circuit/zone data
                if (currentUser?.Jamaat != null)
                {
                    viewModel = await GetCircuitDashboardData(currentUser.Jamaat.CircuitId, activeFiscalYear.Id);
                }
                else
                {
                    // Return empty dashboard if no Jamaat assigned
                    viewModel.AccountSummary = new AccountSummary();
                    viewModel.RecentPayments = new List<Payment>();
                    viewModel.ChandaTypePayments = new Dictionary<string, decimal>();
                    viewModel.MonthlyBudget = new List<decimal>(new decimal[12]);
                    viewModel.MonthlyPayments = new List<decimal>(new decimal[12]);
                }
            }
            else if (User.IsInRole("NationalAdmin") || User.IsInRole("ItAdmin"))
            {
                // National/IT admin can see all data
                viewModel = await GetNationalDashboardData(activeFiscalYear.Id);
            }

            return View(viewModel);
        }

        //private async Task<DashboardViewModel> GetMemberDashboardData(int memberId, int fiscalYearId)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    return new DashboardViewModel
        //    {
        //        AccountSummary = await GetAccountSummary(currentUser.Id, fiscalYearId),
        //        RecentPayments = await GetRecentPayments(currentUser.Id),
        //        ChandaTypePayments = await GetChandaTypePayments(currentUser.Id, fiscalYearId),
        //        MonthlyBudget = (await GetMonthlyData(currentUser.Id, fiscalYearId)).Item1,
        //        MonthlyPayments = (await GetMonthlyData(currentUser.Id, fiscalYearId)).Item2
        //    };
        //}
        private async Task<DashboardViewModel> GetCircuitDashboardData(int circuitId, int fiscalYearId)
        {
            var circuitMembers = await _userManager.Users
                .Where(m => m.Jamaat.CircuitId == circuitId)
                .Select(m => m.Id)
                .ToListAsync();

            return new DashboardViewModel
            {
                AccountSummary = await GetCircuitAccountSummary(circuitId, fiscalYearId),
                RecentPayments = await GetCircuitRecentPayments(circuitId),
                ChandaTypePayments = await GetCircuitChandaTypePayments(circuitId, fiscalYearId),
                MonthlyBudget = (await GetCircuitMonthlyData(circuitId, fiscalYearId)).Item1,
                MonthlyPayments = (await GetCircuitMonthlyData(circuitId, fiscalYearId)).Item2
            };
        }

        private async Task<DashboardViewModel> GetNationalDashboardData(int fiscalYearId)
        {
            return new DashboardViewModel
            {
                AccountSummary = await GetNationalAccountSummary(fiscalYearId),
                RecentPayments = await GetNationalRecentPayments(),
                ChandaTypePayments = await GetNationalChandaTypePayments(fiscalYearId),
                MonthlyBudget = (await GetNationalMonthlyData(fiscalYearId)).Item1,
                MonthlyPayments = (await GetNationalMonthlyData(fiscalYearId)).Item2
            };
        }
        private async Task<AccountSummary> GetCircuitAccountSummary(int circuitId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null)
                return new AccountSummary();
            var currentUser = await _userManager.GetUserAsync(User);

            var totalPayments = await _context.Payments
                .Where(p => currentUser.Jamaat.CircuitId == circuitId &&
                           p.PaymentDate >= _context.FiscalYears.Find(fiscalYearId).StartDate &&
                           p.PaymentDate <= _context.FiscalYears.Find(fiscalYearId).EndDate)
                .SumAsync(p => p.Amount);

            var totalExpectedBudget = await _context.Budgets
                .Where(b => currentUser.Jamaat.CircuitId == circuitId && b.FiscalYearId == fiscalYearId)
                .SumAsync(b => b.Amount);

            return new AccountSummary
            {
                TotalPayments = totalPayments,
                TotalExpectedBudget = totalExpectedBudget,
                BalanceLeft = totalExpectedBudget - totalPayments,
                PercentageComplete = totalExpectedBudget > 0 ? totalPayments / totalExpectedBudget : 0
            };
        }
        private async Task<List<Payment>> GetCircuitRecentPayments(int circuitId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            return await _context.Payments
                .Include(p => p.ChandaType)
                //.Include(p => p.Member)
                .Where(p => currentUser.Jamaat.CircuitId == circuitId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(10) // Show more payments for circuit view
                .ToListAsync();
        }

        private async Task<Dictionary<string, decimal>> GetCircuitChandaTypePayments(int circuitId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null) return new Dictionary<string, decimal>();

            var currentUser = await _userManager.GetUserAsync(User);

            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => currentUser.Jamaat.CircuitId == circuitId &&
                           p.PaymentDate >= fiscalYear.StartDate &&
                           p.PaymentDate <= fiscalYear.EndDate)
                .GroupBy(p => p.ChandaType.Name)
                .Select(g => new { ChandaType = g.Key, Total = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.ChandaType, x => x.Total);
        }

        private async Task<(List<decimal>, List<decimal>)> GetCircuitMonthlyData(int circuitId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null) return (new List<decimal>(new decimal[12]), new List<decimal>(new decimal[12]));

            var currentUser = await _userManager.GetUserAsync(User);

            // Get monthly budget for circuit
            var monthlyBudget = await _context.Budgets
                .Where(b => currentUser.Jamaat.CircuitId == circuitId &&
                           b.FiscalYearId == fiscalYearId)
                .GroupBy(b => b.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(b => b.Amount))
                .ToListAsync();

            // Ensure we have 12 months of budget data
            while (monthlyBudget.Count < 12)
            {
                monthlyBudget.Add(0);
            }

            // Get monthly payments for circuit
            var monthlyPayments = new List<decimal>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(fiscalYear.StartDate.Year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var total = await _context.Payments
                    .Where(p => currentUser.Jamaat.CircuitId == circuitId &&
                               p.PaymentDate >= startDate &&
                               p.PaymentDate <= endDate)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                monthlyPayments.Add(total);
            }

            return (monthlyBudget, monthlyPayments);
        }
        // National-level data methods
        private async Task<AccountSummary> GetNationalAccountSummary(int fiscalYearId)
        {
            var totalPayments = await _context.Payments
                .Where(p => p.PaymentDate >= _context.FiscalYears.Find(fiscalYearId).StartDate &&
                           p.PaymentDate <= _context.FiscalYears.Find(fiscalYearId).EndDate)
                .SumAsync(p => p.Amount);

            var totalExpectedBudget = await _context.Budgets
                .Where(b => b.FiscalYearId == fiscalYearId)
                .SumAsync(b => b.Amount);

            return new AccountSummary
            {
                TotalPayments = totalPayments,
                TotalExpectedBudget = totalExpectedBudget,
                BalanceLeft = totalExpectedBudget - totalPayments,
                PercentageComplete = totalExpectedBudget > 0 ? totalPayments / totalExpectedBudget : 0
            };
        }
        private async Task<List<Payment>> GetNationalRecentPayments()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            return await _context.Payments
                .Include(p => p.ChandaType)
                .OrderByDescending(p => p.PaymentDate)
                .Take(15) // Show more payments for national view
                .ToListAsync();
        }

        private async Task<Dictionary<string, decimal>> GetNationalChandaTypePayments(int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null) return new Dictionary<string, decimal>();

            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => p.PaymentDate >= fiscalYear.StartDate &&
                           p.PaymentDate <= fiscalYear.EndDate)
                .GroupBy(p => p.ChandaType.Name)
                .Select(g => new { ChandaType = g.Key, Total = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.ChandaType, x => x.Total);
        }

        private async Task<(List<decimal>, List<decimal>)> GetNationalMonthlyData(int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null) return (new List<decimal>(new decimal[12]), new List<decimal>(new decimal[12]));

            // Get national monthly budget
            var monthlyBudget = await _context.Budgets
                .Where(b => b.FiscalYearId == fiscalYearId)
                .GroupBy(b => b.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(b => b.Amount))
                .ToListAsync();

            // Ensure we have 12 months of budget data
            while (monthlyBudget.Count < 12)
            {
                monthlyBudget.Add(0);
            }

            // Get national monthly payments
            var monthlyPayments = new List<decimal>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(fiscalYear.StartDate.Year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var total = await _context.Payments
                    .Where(p => p.PaymentDate >= startDate &&
                               p.PaymentDate <= endDate)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                monthlyPayments.Add(total);
            }

            return (monthlyBudget, monthlyPayments);
        }
        private async Task<AccountSummary> GetAccountSummary(string userId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);
            if (fiscalYear == null)
                return null;

            var totalPayments = await _context.Payments
                .Where(p => p.UserId == userId &&
                            p.PaymentDate >= fiscalYear.StartDate &&
                            p.PaymentDate <= fiscalYear.EndDate)
                .SumAsync(p => p.Amount);

            var totalExpectedBudget = await _context.Budgets
                .Where(b => b.UserId == userId && b.FiscalYearId == fiscalYearId)
                .SumAsync(b => b.Amount);

            return new AccountSummary
            {
                TotalPayments = totalPayments,
                TotalExpectedBudget = totalExpectedBudget,
                BalanceLeft = totalExpectedBudget - totalPayments,
                PercentageComplete = totalExpectedBudget > 0 ? totalPayments / totalExpectedBudget : 0
            };
        }

        private async Task<List<Payment>> GetRecentPayments(string userId)
        {
            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .ToListAsync();
        }


        private async Task<Dictionary<string, decimal>> GetChandaTypePayments(string userId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);

            return await _context.Payments
                .Include(p => p.ChandaType)
                .Where(p => p.UserId == userId &&
                            p.PaymentDate >= fiscalYear.StartDate &&
                            p.PaymentDate <= fiscalYear.EndDate)
                .GroupBy(p => p.ChandaType.Name)
                .Select(g => new { ChandaType = g.Key, Total = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.ChandaType, x => x.Total);
        }

        private async Task<(List<decimal>, List<decimal>)> GetMonthlyData(string userId, int fiscalYearId)
        {
            var fiscalYear = await _context.FiscalYears.FindAsync(fiscalYearId);

            // Monthly budget
            var monthlyBudget = await _context.Budgets
                .Where(b => b.UserId == userId && b.FiscalYearId == fiscalYearId)
                .GroupBy(b => b.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(b => b.Amount))
                .ToListAsync();

            while (monthlyBudget.Count < 12)
                monthlyBudget.Add(0);

            // Monthly payments
            var monthlyPayments = new List<decimal>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(fiscalYear.StartDate.Year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var total = await _context.Payments
                    .Where(p => p.UserId == userId &&
                                p.PaymentDate >= startDate &&
                                p.PaymentDate <= endDate)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                monthlyPayments.Add(total);
            }

            return (monthlyBudget, monthlyPayments);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
