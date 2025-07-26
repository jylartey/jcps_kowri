using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class Budget
{
    internal decimal? TotalProjectedAmount;

    [Key]
    public int Id { get; set; }
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
    public int ChandaTypeId { get; set; }
    public decimal AmountPaid { get; set; }
    public int FiscalYearId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }


    // Navigation Properties
    public Member Member { get; set; }
    public ChandaType ChandaType { get; set; }
    public FiscalYear FiscalYear { get; set; }
}

