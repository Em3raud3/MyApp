using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Controllers;

public class FoodController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Recipes()
    {
        var recipes = await db.Recipes.Include(r => r.Ingredients).ToListAsync();
        return View(recipes);
    }

    // ── Ingredients ──────────────────────────────────────────────

    public async Task<IActionResult> Ingredients()
    {
        var ingredients = await db.Ingredients.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();
        return View(ingredients);
    }

    [HttpGet]
    public IActionResult CreateIngredient() => View("IngredientForm", new Ingredient());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateIngredient(Ingredient model)
    {
        if (!ModelState.IsValid) return View("IngredientForm", model);
        db.Ingredients.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Ingredients));
    }

    [HttpGet]
    public async Task<IActionResult> EditIngredient(int id)
    {
        var ingredient = await db.Ingredients.FindAsync(id);
        if (ingredient is null) return NotFound();
        return View("IngredientForm", ingredient);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditIngredient(int id, Ingredient model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("IngredientForm", model);
        db.Ingredients.Update(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Ingredients));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteIngredient(int id)
    {
        var ingredient = await db.Ingredients.FindAsync(id);
        if (ingredient is not null)
        {
            db.Ingredients.Remove(ingredient);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Ingredients));
    }

    // ── Shopping List ─────────────────────────────────────────────

    public async Task<IActionResult> ShoppingList()
    {
        var ingredients = await db.Ingredients.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();
        return View(ingredients);
    }
}
