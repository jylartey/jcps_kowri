using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class PaymentMethod
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Rule { get; set; }
}
