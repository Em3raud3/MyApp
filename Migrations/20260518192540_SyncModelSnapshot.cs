using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MealPlanId",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MealPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeekOf = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlans", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_MealPlans_MealPlanId",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "MealPlans");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_MealPlanId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "MealPlanId",
                table: "Recipes");
        }
    }
}
