using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitFuel.Api.Migrations
{
    /// <inheritdoc />
    public partial class removeadjustedrecipemeasurementcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdjustedRecipes_RecipeId_Measurement",
                table: "AdjustedRecipes");

            migrationBuilder.DropColumn(
                name: "Measurement",
                table: "AdjustedRecipes");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustedRecipes_RecipeId_Servings",
                table: "AdjustedRecipes",
                columns: new[] { "RecipeId", "Servings" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdjustedRecipes_RecipeId_Servings",
                table: "AdjustedRecipes");

            migrationBuilder.AddColumn<string>(
                name: "Measurement",
                table: "AdjustedRecipes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustedRecipes_RecipeId_Measurement",
                table: "AdjustedRecipes",
                columns: new[] { "RecipeId", "Measurement" },
                unique: true);
        }
    }
}
