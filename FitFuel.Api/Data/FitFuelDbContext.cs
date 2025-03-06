using Microsoft.EntityFrameworkCore;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data
{
    public class FitFuelDbContext : DbContext
    {
        public FitFuelDbContext(DbContextOptions<FitFuelDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemType> ItemTypes { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<RecipeItem> RecipeItems { get; set; } = null!;
        public DbSet<AdjustedItem> AdjustedItems { get; set; } = null!;
        public DbSet<AdjustedRecipe> AdjustedRecipes { get; set; } = null!;       // Add your DbSet properties here


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize entity mappings here if needed
        }
    }
}
