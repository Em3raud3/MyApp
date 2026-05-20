using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameSnacksToQuickEats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the FK that references PK_Snacks so we can rename the table
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanSnack_Snacks_SnacksId",
                table: "MealPlanSnack");

            // 2. Rename join table and its column/index
            migrationBuilder.RenameTable(
                name: "MealPlanSnack",
                newName: "MealPlanQuickEat");

            migrationBuilder.RenameColumn(
                name: "SnacksId",
                table: "MealPlanQuickEat",
                newName: "QuickEatsId");

            migrationBuilder.RenameIndex(
                name: "IX_MealPlanSnack_SnacksId",
                table: "MealPlanQuickEat",
                newName: "IX_MealPlanQuickEat_QuickEatsId");

            // 3. Rename main entity table (PK drop/rename/re-add is now safe)
            migrationBuilder.DropPrimaryKey(
                name: "PK_Snacks",
                table: "Snacks");

            migrationBuilder.RenameTable(
                name: "Snacks",
                newName: "QuickEats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuickEats",
                table: "QuickEats",
                column: "Id");

            // 4. Re-add the FK pointing at the renamed table/column
            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanQuickEat_QuickEats_QuickEatsId",
                table: "MealPlanQuickEat",
                column: "QuickEatsId",
                principalTable: "QuickEats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the FK so we can rename back
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanQuickEat_QuickEats_QuickEatsId",
                table: "MealPlanQuickEat");

            // 2. Rename main table back
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuickEats",
                table: "QuickEats");

            migrationBuilder.RenameTable(
                name: "QuickEats",
                newName: "Snacks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Snacks",
                table: "Snacks",
                column: "Id");

            // 3. Rename join table and its column/index back
            migrationBuilder.RenameIndex(
                name: "IX_MealPlanQuickEat_QuickEatsId",
                table: "MealPlanQuickEat",
                newName: "IX_MealPlanSnack_SnacksId");

            migrationBuilder.RenameColumn(
                name: "QuickEatsId",
                table: "MealPlanQuickEat",
                newName: "SnacksId");

            migrationBuilder.RenameTable(
                name: "MealPlanQuickEat",
                newName: "MealPlanSnack");

            // 4. Re-add the FK pointing at the restored Snacks table
            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanSnack_Snacks_SnacksId",
                table: "MealPlanSnack",
                column: "SnacksId",
                principalTable: "Snacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
