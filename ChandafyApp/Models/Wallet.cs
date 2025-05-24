using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class Wallet
{
    [Key]
    public int Id { get; set; }
    public decimal BalanceLeft { get; set; }
}
