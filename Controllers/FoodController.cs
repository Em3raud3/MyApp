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
    public async Task<IActionResult> CreateMealPlan(string name, DateOnly weekOf, int[] selectedRecipes, int[] selectedQuickEats, int[] selectedDrinks)
    {
        var plan = new MealPlan { Name = name, WeekOf = weekOf };
        plan.Recipes = await db.Recipes.Where(r => selectedRecipes.Contains(r.Id)).ToListAsync();
        plan.QuickEats = await db.QuickEats.Where(q => selectedQuickEats.Contains(q.Id)).ToListAsync();
        plan.Drinks = await db.Drinks.Where(d => selectedDrinks.Contains(d.Id)).ToListAsync();
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

        if (plan.QuickEats.Any())
        {
            lines.AppendLine("QUICK EATS");
            foreach (var quickEat in plan.QuickEats.OrderBy(q => q.Name))
                lines.AppendLine($"  - {quickEat.Name}");
            lines.AppendLine();
        }

        try
        {
            await email.SendAsync($"🛒 Shopping List — {plan.Name}", lines.ToString());
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
