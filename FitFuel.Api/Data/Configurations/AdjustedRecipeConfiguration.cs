using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data.Configurations
{
    public class AdjustedRecipeConfiguration : IEntityTypeConfiguration<AdjustedRecipe>
    {
        public void Configure(EntityTypeBuilder<AdjustedRecipe> builder)
        {
            builder.ToTable("AdjustedRecipes");
            
            builder.HasKey(ar => ar.Id);
            
            builder.Property(ar => ar.Measurement)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(ar => ar.Servings)
                .IsRequired();
                
            // Relationships
            builder.HasOne(ar => ar.Recipe)
                .WithMany()
                .HasForeignKey(ar => ar.RecipeId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
                
            // Add index to prevent duplicate measurements for the same recipe
            builder.HasIndex(ar => new { ar.RecipeId, ar.Measurement })
                .IsUnique();
        }
    }
}