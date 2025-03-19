using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data.Configurations
{
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.ToTable("Recipes");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Servings)
                .IsRequired();

            builder.HasIndex(r => r.Name)
                .IsUnique();
        }
    }
}