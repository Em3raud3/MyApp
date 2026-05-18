using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;

namespace MyApp.Controllers;

public class FoodController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Recipes()
    {
        var recipes = await db.Recipes.Include(r => r.Ingredients).ToListAsync();
        return View(recipes);
    }

    public async Task<IActionResult> Ingredients()
    {
        var ingredients = await db.Ingredients.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();
        return View(ingredients);
    }

    public async Task<IActionResult> ShoppingList()
    {
        var ingredients = await db.Ingredients.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();
        return View(ingredients);
    }
}
