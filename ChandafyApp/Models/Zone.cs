
using System.ComponentModel.DataAnnotations;
namespace ChandafyApp.Models;
public class Zone
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int RegionId { get; set; }

    // Navigation Properties
    public Region Region { get; set; }
    public ICollection<Circuit> Circuits { get; set; }
}
