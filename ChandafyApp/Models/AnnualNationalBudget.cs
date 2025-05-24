using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class AnnualNationalBudget
{
    [Key]
    public int Id { get; set; }
    public string TotalBudgetType { get; set; }
    public decimal TotalBudgetAmount { get; set; }
    public decimal TotalExpectedBudgetAmount { get; set; }
    public decimal TotalCollectedAmount { get; set; }
    public int FiscalYearId { get; set; }

    // Navigation Property
    public FiscalYear FiscalYear { get; set; }
}
