using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<QuickEat> QuickEats => Set<QuickEat>();
    public DbSet<Drink> Drinks => Set<Drink>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MealPlan>()
            .HasMany(p => p.Recipes)
            .WithMany(r => r.MealPlans)
            .UsingEntity("MealPlanRecipe");

        modelBuilder.Entity<MealPlan>()
            .HasMany(p => p.QuickEats)
            .WithMany(q => q.MealPlans)
            .UsingEntity("MealPlanQuickEat");

        modelBuilder.Entity<MealPlan>()
            .HasMany(p => p.Drinks)
            .WithMany()
            .UsingEntity("MealPlanDrink");
    }
}
