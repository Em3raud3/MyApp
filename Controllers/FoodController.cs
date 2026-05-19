using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Controllers;

public class FoodController(AppDbContext db, EmailService email) : Controller
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

    // ── Meal Plan ─────────────────────────────────────────────────

    public async Task<IActionResult> MealPlan()
    {
        var plans = await db.MealPlans
            .Include(p => p.Recipes)
            .OrderByDescending(p => p.WeekOf)
            .ToListAsync();
        ViewBag.AllRecipes = await db.Recipes.OrderBy(r => r.Name).ToListAsync();
        return View(plans);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMealPlan(string name, DateOnly weekOf, int[] selectedRecipes)
    {
        var plan = new MealPlan { Name = name, WeekOf = weekOf };
        plan.Recipes = await db.Recipes.Where(r => selectedRecipes.Contains(r.Id)).ToListAsync();
        db.MealPlans.Add(plan);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(MealPlan));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMealPlan(int id)
    {
        var plan = await db.MealPlans.FindAsync(id);
        if (plan is not null)
        {
            db.MealPlans.Remove(plan);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(MealPlan));
    }

    // ── Shopping List ─────────────────────────────────────────────

    public async Task<IActionResult> ShoppingList(int? planId)
    {
        var plans = await db.MealPlans.OrderByDescending(p => p.WeekOf).ToListAsync();
        MealPlan? selected = null;

        if (planId.HasValue)
        {
            selected = await db.MealPlans
                .Include(p => p.Recipes).ThenInclude(r => r.Ingredients)
                .FirstOrDefaultAsync(p => p.Id == planId.Value);
        }
        else if (plans.Count > 0)
        {
            selected = await db.MealPlans
                .Include(p => p.Recipes).ThenInclude(r => r.Ingredients)
                .FirstOrDefaultAsync(p => p.Id == plans[0].Id);
        }

        ViewBag.Plans = plans;
        ViewBag.SelectedPlan = selected;
        return View(selected);
    }

    // ── Email Shopping List ───────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EmailShoppingList(int planId)
    {
        var plan = await db.MealPlans
            .Include(p => p.Recipes).ThenInclude(r => r.Ingredients)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan is null) return NotFound();

        var ingredients = plan.Recipes
            .SelectMany(r => r.Ingredients)
            .DistinctBy(i => i.Id)
            .OrderBy(i => i.Category)
            .ThenBy(i => i.Name)
            .GroupBy(i => i.Category ?? "Uncategorized");

        var lines = new System.Text.StringBuilder();
        lines.AppendLine($"Shopping list for: {plan.Name}");
        lines.AppendLine($"Week of {plan.WeekOf:MMMM d, yyyy}");
        lines.AppendLine();

        foreach (var group in ingredients)
        {
            lines.AppendLine(group.Key.ToUpper());
            foreach (var ingredient in group)
                lines.AppendLine($"  - {ingredient.Name}");
            lines.AppendLine();
        }

        await email.SendAsync($"🛒 Shopping List — {plan.Name}", lines.ToString());

        TempData["EmailSent"] = "Shopping list sent to your email!";
        return RedirectToAction(nameof(ShoppingList), new { planId });
    }
}
