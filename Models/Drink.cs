using System.ComponentModel.DataAnnotations;

namespace MyApp.Models;

public class Drink
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}
