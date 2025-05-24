using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class Income
{
    [Key]
    public int Id { get; set; }
    public string IncomeType { get; set; }
    public decimal Amount { get; set; }
    public int FiscalYearId { get; set; }

    // Navigation Property
    public FiscalYear FiscalYear { get; set; }
}
