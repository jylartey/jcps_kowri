using System.ComponentModel.DataAnnotations;
namespace ChandafyApp.Models;
public class Region
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation Property
    public ICollection<Zone> Zones { get; set; }
}
