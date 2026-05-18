using System.ComponentModel.DataAnnotations;

namespace MyApp.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    public string? Category { get; set; }
}
