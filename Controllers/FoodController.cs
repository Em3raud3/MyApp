using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Controllers;

public class FoodController(AppDbContext db, EmailService email) : Controller
{
    public async Task<IActionResult> Cravings()
    {
        var recipes = await db.Recipes.Include(r => r.Ingredients).ToListAsync();
        var quickEats = await db.QuickEats.ToListAsync();
        var drinks = await db.Drinks.ToListAsync();

        var rng = Random.Shared;
        var shuffledRecipes = recipes.OrderBy(_ => rng.Next()).ToList();
        var shuffledQuickEats = quickEats.OrderBy(_ => rng.Next()).ToList();
        var shuffledDrinks = drinks.OrderBy(_ => rng.Next()).ToList();

        ViewBag.QuickEats = shuffledQuickEats;
        ViewBag.Drinks = shuffledDrinks;
        return View(shuffledRecipes);
    }

    [HttpGet]
    public async Task<IActionResult> CreateRecipe()
    {
        ViewBag.AllIngredients = await GetAllIngredientsAsync();
        return View("RecipeForm", new Recipe());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRecipe(Recipe model, int[] selectedIngredients)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllIngredients = await GetAllIngredientsAsync();
            return View("RecipeForm", model);
        }
        model.Ingredients = await db.Ingredients.Where(i => selectedIngredients.Contains(i.Id)).ToListAsync();
        db.Recipes.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Cravings));
    }

    [HttpGet]
    public async Task<IActionResult> EditRecipe(int id)
    {
        var recipe = await db.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null) return NotFound();
        ViewBag.AllIngredients = await GetAllIngredientsAsync();
        return View("RecipeForm", recipe);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRecipe(int id, Recipe model, int[] selectedIngredients)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.AllIngredients = await GetAllIngredientsAsync();
            return View("RecipeForm", model);
        }
        var recipe = await db.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == id);
        if (recipe is null) return NotFound();
        recipe.Name = model.Name;
        recipe.Description = model.Description;
        recipe.ImageUrl = model.ImageUrl;
        recipe.Instructions = model.Instructions;

        var ids = selectedIngredients ?? [];
        var newIngredients = await db.Ingredients.Where(i => ids.Contains(i.Id)).ToListAsync();
        recipe.Ingredients.Clear();
        foreach (var ingredient in newIngredients)
            recipe.Ingredients.Add(ingredient);

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Cravings));
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
        return RedirectToAction(nameof(Cravings));
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
            .Include(p => p.QuickEats)
            .Include(p => p.Drinks)
            .OrderByDescending(p => p.WeekOf)
            .ToListAsync();
        ViewBag.AllRecipes = await db.Recipes.OrderBy(r => r.Name).ToListAsync();
        ViewBag.AllQuickEats = await db.QuickEats.OrderBy(q => q.Name).ToListAsync();
        ViewBag.AllDrinks = await db.Drinks.OrderBy(d => d.Name).ToListAsync();
        return View(plans);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMealPlan(string name, DateOnly weekOf, string? notes, int[] selectedRecipes, int[] selectedQuickEats, int[] selectedDrinks)
    {
        var plan = new MealPlan { Name = name, WeekOf = weekOf, Notes = notes };
        plan.Recipes = await db.Recipes.Where(r => selectedRecipes.Contains(r.Id)).ToListAsync();
        plan.QuickEats = await db.QuickEats.Where(q => selectedQuickEats.Contains(q.Id)).ToListAsync();
        plan.Drinks = await db.Drinks.Where(d => selectedDrinks.Contains(d.Id)).ToListAsync();
        db.MealPlans.Add(plan);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(MealPlan));
    }

    [HttpGet]
    public async Task<IActionResult> EditMealPlan(int id)
    {
        var plan = await db.MealPlans
            .Include(p => p.Recipes)
            .Include(p => p.QuickEats)
            .Include(p => p.Drinks)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (plan is null) return NotFound();
        ViewBag.AllRecipes = await db.Recipes.OrderBy(r => r.Name).ToListAsync();
        ViewBag.AllQuickEats = await db.QuickEats.OrderBy(q => q.Name).ToListAsync();
        ViewBag.AllDrinks = await db.Drinks.OrderBy(d => d.Name).ToListAsync();
        return View(plan);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMealPlan(int id, string name, DateOnly weekOf, string? notes, int[] selectedRecipes, int[] selectedQuickEats, int[] selectedDrinks)
    {
        var plan = await db.MealPlans
            .Include(p => p.Recipes)
            .Include(p => p.QuickEats)
            .Include(p => p.Drinks)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (plan is null) return NotFound();

        plan.Name = name;
        plan.WeekOf = weekOf;
        plan.Notes = notes;
        plan.Recipes = await db.Recipes.Where(r => selectedRecipes.Contains(r.Id)).ToListAsync();
        plan.QuickEats = await db.QuickEats.Where(q => selectedQuickEats.Contains(q.Id)).ToListAsync();
        plan.Drinks = await db.Drinks.Where(d => selectedDrinks.Contains(d.Id)).ToListAsync();

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

    // ── Quick Eats ───────────────────────────────────────────────

    public async Task<IActionResult> QuickEats()
    {
        var quickEats = await db.QuickEats.OrderBy(q => q.Name).ToListAsync();
        return View(quickEats);
    }

    [HttpGet]
    public IActionResult CreateQuickEat() => View("QuickEatForm", new QuickEat());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuickEat(QuickEat model)
    {
        if (!ModelState.IsValid) return View("QuickEatForm", model);
        db.QuickEats.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Cravings));
    }

    [HttpGet]
    public async Task<IActionResult> EditQuickEat(int id)
    {
        var quickEat = await db.QuickEats.FindAsync(id);
        if (quickEat is null) return NotFound();
        return View("QuickEatForm", quickEat);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuickEat(int id, QuickEat model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("QuickEatForm", model);
        db.QuickEats.Update(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(QuickEats));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuickEat(int id)
    {
        var quickEat = await db.QuickEats.FindAsync(id);
        if (quickEat is not null)
        {
            db.QuickEats.Remove(quickEat);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(QuickEats));
    }

    // ── Shopping List ─────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePlanQuickEats(int planId, int[] selectedQuickEats)
    {
        var plan = await db.MealPlans.Include(p => p.QuickEats).FirstOrDefaultAsync(p => p.Id == planId);
        if (plan is null) return NotFound();
        var chosen = await db.QuickEats.Where(q => selectedQuickEats.Contains(q.Id)).ToListAsync();
        plan.QuickEats.Clear();
        foreach (var quickEat in chosen)
            plan.QuickEats.Add(quickEat);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(ShoppingList), new { planId });
    }

    public async Task<IActionResult> ShoppingList(int? planId)
    {
        var plans = await db.MealPlans.OrderByDescending(p => p.WeekOf).ToListAsync();
        var targetId = planId ?? plans.FirstOrDefault()?.Id;

        MealPlan? selected = targetId is null
            ? null
            : await db.MealPlans
                .AsSplitQuery()
                .Include(p => p.Recipes).ThenInclude(r => r.Ingredients)
                .Include(p => p.QuickEats)
                .Include(p => p.Drinks)
                .FirstOrDefaultAsync(p => p.Id == targetId);

        ViewBag.Plans = plans;
        ViewBag.SelectedPlan = selected;
        ViewBag.AllQuickEats = await db.QuickEats.OrderBy(q => q.Name).ToListAsync();
        return View(selected);
    }

    // ── Email Shopping List ───────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EmailShoppingList(int planId)
    {
        var plan = await db.MealPlans
            .Include(p => p.Recipes).ThenInclude(r => r.Ingredients)
            .Include(p => p.QuickEats)
            .Include(p => p.Drinks)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan is null) return NotFound();

        var ingredients = plan.Recipes
            .SelectMany(r => r.Ingredients)
            .DistinctBy(i => i.Id)
            .OrderBy(i => i.Category)
            .ThenBy(i => i.Name)
            .GroupBy(i => i.Category ?? "Uncategorized");

        // ── Plain-text body ───────────────────────────────────────
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

        if (plan.QuickEats.Any())
        {
            lines.AppendLine("QUICK EATS");
            foreach (var quickEat in plan.QuickEats.OrderBy(q => q.Name))
                lines.AppendLine($"  - {quickEat.Name}");
            lines.AppendLine();
        }

        if (plan.Drinks.Any())
        {
            lines.AppendLine("DRINKS");
            foreach (var drink in plan.Drinks.OrderBy(d => d.Name))
                lines.AppendLine($"  - {drink.Name}");
            lines.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(plan.Notes))
        {
            lines.AppendLine("NOTES");
            lines.AppendLine(plan.Notes);
            lines.AppendLine();
        }

        // ── HTML body with checkboxes ─────────────────────────────
        var html = new System.Text.StringBuilder();
        html.AppendLine("""
            <!DOCTYPE html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width,initial-scale=1">
            <title>Shopping List</title>
            <style>
              body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background:#1a1a1a; color:#e0e0e0; margin:0; padding:16px; }
              h1 { font-size:1.4em; color:#a855f7; margin:0 0 4px; }
              .week { font-size:0.85em; color:#888; margin:0 0 24px; }
              h2 { font-size:0.75em; font-weight:700; letter-spacing:0.1em; text-transform:uppercase; color:#a855f7; border-bottom:1px solid #333; padding-bottom:4px; margin:24px 0 8px; }
              ul { list-style:none; margin:0; padding:0; }
              li { display:flex; align-items:center; gap:10px; padding:6px 0; border-bottom:1px solid #2a2a2a; }
              li:last-child { border-bottom:none; }
              input[type=checkbox] { width:18px; height:18px; accent-color:#a855f7; flex-shrink:0; cursor:pointer; }
              label { font-size:0.95em; cursor:pointer; }
              input[type=checkbox]:checked + label { text-decoration:line-through; color:#555; }
              .notes { background:#222; border-left:3px solid #a855f7; padding:10px 14px; margin-top:24px; font-size:0.875em; white-space:pre-wrap; color:#c0c0c0; }
            </style>
            </head>
            <body>
            """);

        html.AppendLine($"<h1>🛒 {System.Web.HttpUtility.HtmlEncode(plan.Name)}</h1>");
        html.AppendLine($"<p class=\"week\">Week of {plan.WeekOf:MMMM d, yyyy}</p>");

        int uid = 0;
        foreach (var group in ingredients)
        {
            html.AppendLine($"<h2>{System.Web.HttpUtility.HtmlEncode(group.Key)}</h2><ul>");
            foreach (var ingredient in group)
            {
                uid++;
                html.AppendLine($"<li><input type=\"checkbox\" id=\"i{uid}\"><label for=\"i{uid}\">{System.Web.HttpUtility.HtmlEncode(ingredient.Name)}</label></li>");
            }
            html.AppendLine("</ul>");
        }

        if (plan.QuickEats.Any())
        {
            html.AppendLine("<h2>Quick Eats</h2><ul>");
            foreach (var quickEat in plan.QuickEats.OrderBy(q => q.Name))
            {
                uid++;
                html.AppendLine($"<li><input type=\"checkbox\" id=\"i{uid}\"><label for=\"i{uid}\">{System.Web.HttpUtility.HtmlEncode(quickEat.Name)}</label></li>");
            }
            html.AppendLine("</ul>");
        }

        if (plan.Drinks.Any())
        {
            html.AppendLine("<h2>Drinks</h2><ul>");
            foreach (var drink in plan.Drinks.OrderBy(d => d.Name))
            {
                uid++;
                html.AppendLine($"<li><input type=\"checkbox\" id=\"i{uid}\"><label for=\"i{uid}\">{System.Web.HttpUtility.HtmlEncode(drink.Name)}</label></li>");
            }
            html.AppendLine("</ul>");
        }

        if (!string.IsNullOrWhiteSpace(plan.Notes))
            html.AppendLine($"<div class=\"notes\"><strong>Notes</strong><br>{System.Web.HttpUtility.HtmlEncode(plan.Notes)}</div>");

        html.AppendLine("</body></html>");

        try
        {
            await email.SendAsync($"🛒 Shopping List — {plan.Name}", lines.ToString(), html.ToString());
            TempData["EmailSent"] = "Shopping list sent to your email!";
        }
        catch (Exception ex)
        {
            TempData["EmailError"] = $"Could not send email: {ex.Message}";
        }

        return RedirectToAction(nameof(ShoppingList), new { planId });
    }

    // ── Drinks ───────────────────────────────────────────────────

    public async Task<IActionResult> Drinks()
    {
        var drinks = await db.Drinks.OrderBy(d => d.Name).ToListAsync();
        return View(drinks);
    }

    [HttpGet]
    public IActionResult CreateDrink() => View("DrinkForm", new Drink());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDrink(Drink model)
    {
        if (!ModelState.IsValid) return View("DrinkForm", model);
        db.Drinks.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Cravings));
    }

    [HttpGet]
    public async Task<IActionResult> EditDrink(int id)
    {
        var drink = await db.Drinks.FindAsync(id);
        if (drink is null) return NotFound();
        return View("DrinkForm", drink);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDrink(int id, Drink model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("DrinkForm", model);
        db.Drinks.Update(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Drinks));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDrink(int id)
    {
        var drink = await db.Drinks.FindAsync(id);
        if (drink is not null)
        {
            db.Drinks.Remove(drink);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Cravings));
    }

    private Task<List<Ingredient>> GetAllIngredientsAsync() =>
        db.Ingredients.OrderBy(i => i.Name).ToListAsync();
}
