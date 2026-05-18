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

    [HttpGet]
    public async Task<IActionResult> CreateRecipe()
    {
        ViewBag.AllIngredients = await db.Ingredients.OrderBy(i => i.Name).ToListAsync();
        return View("RecipeForm", new Recipe());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRecipe(Recipe model, int[] selectedIngredients)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllIngredients = await db.Ingredients.OrderBy(i => i.Name).ToListAsync();
            return View("RecipeForm", model);
        }
        model.Ingredients = await db.Ingredients.Where(i => selectedIngredients.Contains(i.Id)).ToListAsync();
        db.Recipes.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Recipes));
    }

    [HttpGet]
    public async Task<IActionResult> EditRecipe(int id)
    {
        var recipe = await db.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null) return NotFound();
        ViewBag.AllIngredients = await db.Ingredients.OrderBy(i => i.Name).ToListAsync();
        return View("RecipeForm", recipe);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRecipe(int id, Recipe model, int[] selectedIngredients)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.AllIngredients = await db.Ingredients.OrderBy(i => i.Name).ToListAsync();
            return View("RecipeForm", model);
        }
        var recipe = await db.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null) return NotFound();
        recipe.Name = model.Name;
        recipe.Description = model.Description;
        recipe.ImageUrl = model.ImageUrl;
        recipe.Instructions = model.Instructions;
        recipe.Ingredients = await db.Ingredients.Where(i => selectedIngredients.Contains(i.Id)).ToListAsync();
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Recipes));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        var recipe = await db.Recipes.FindAsync(id);
        if (recipe is not null)
        {
            db.Recipes.Remove(recipe);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Recipes));
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
