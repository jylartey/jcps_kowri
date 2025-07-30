using ChandafyApp.Data;
using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class Jamaat
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int CircuitId { get; set; }

    // Navigation Properties
    public Circuit Circuit { get; set; }
    public ICollection<ApplicationUser> Users { get; set; }
}

