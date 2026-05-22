namespace MyApp.Models;

public class MealPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly WeekOf { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Recipe> Recipes { get; set; } = [];
    public ICollection<QuickEat> QuickEats { get; set; } = [];
    public ICollection<Drink> Drinks { get; set; } = [];
}
