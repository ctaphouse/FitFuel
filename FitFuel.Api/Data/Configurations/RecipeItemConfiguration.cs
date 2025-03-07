using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data.Configurations
{
    public class RecipeItemConfiguration : IEntityTypeConfiguration<RecipeItem>
    {
        public void Configure(EntityTypeBuilder<RecipeItem> builder)
        {
            builder.ToTable("RecipeItems");
            
            builder.HasKey(ri => ri.Id);
            
            builder.Property(ri => ri.Measurement)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(ri => ri.GramEquivalent)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            // Relationships
            builder.HasOne(ri => ri.Recipe)
                .WithMany()
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
                
            builder.HasOne(ri => ri.Item)
                .WithMany()
                .HasForeignKey(ri => ri.ItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}