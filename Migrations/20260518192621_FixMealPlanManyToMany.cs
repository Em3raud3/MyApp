using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class FixMealPlanManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_MealPlans_MealPlanId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_MealPlanId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "MealPlanId",
                table: "Recipes");

            migrationBuilder.CreateTable(
                name: "MealPlanRecipe",
                columns: table => new
                {
                    MealPlansId = table.Column<int>(type: "int", nullable: false),
                    RecipesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlanRecipe", x => new { x.MealPlansId, x.RecipesId });
                    table.ForeignKey(
                        name: "FK_MealPlanRecipe_MealPlans_MealPlansId",
                        column: x => x.MealPlansId,
                        principalTable: "MealPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealPlanRecipe_Recipes_RecipesId",
                        column: x => x.RecipesId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanRecipe_RecipesId",
                table: "MealPlanRecipe",
                column: "RecipesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealPlanRecipe");

            migrationBuilder.AddColumn<int>(
                name: "MealPlanId",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_MealPlanId",
                table: "Recipes",
                column: "MealPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_MealPlans_MealPlanId",
                table: "Recipes",
                column: "MealPlanId",
                principalTable: "MealPlans",
                principalColumn: "Id");
        }
    }
}
