// Add to FitFuel.Api/Data/FitFuelDbContext.cs

using Microsoft.EntityFrameworkCore;
using FitFuel.Api.Models;
using FitFuel.Api.Data.Configurations;

namespace FitFuel.Api.Data
{
    public class FitFuelDbContext : DbContext
    {
        public FitFuelDbContext(DbContextOptions<FitFuelDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for all entities
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<ItemType> ItemTypes { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<RecipeItem> RecipeItems { get; set; } = null!;
        public DbSet<AdjustedItem> AdjustedItems { get; set; } = null!;
        public DbSet<AdjustedRecipe> AdjustedRecipes { get; set; } = null!;
        
        // Add grocery list entities
        public DbSet<GroceryList> GroceryLists { get; set; } = null!;
        public DbSet<GroceryListItem> GroceryListItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new ItemTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new RecipeConfiguration());
            modelBuilder.ApplyConfiguration(new RecipeItemConfiguration());
            modelBuilder.ApplyConfiguration(new AdjustedItemConfiguration());
            modelBuilder.ApplyConfiguration(new AdjustedRecipeConfiguration());
            
            // Apply grocery list configurations
            modelBuilder.ApplyConfiguration(new GroceryListConfiguration());
            modelBuilder.ApplyConfiguration(new GroceryListItemConfiguration());
        }
    }
}