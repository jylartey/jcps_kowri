using System.ComponentModel.DataAnnotations;
namespace ChandafyApp.Models;

public class Circuit
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int ZoneId { get; set; }

    // Navigation Properties
    public Zone Zone { get; set; }
    public ICollection<Jamaat> Jamaats { get; set; }
}

