using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data.Configurations
{
    public class AdjustedItemConfiguration : IEntityTypeConfiguration<AdjustedItem>
    {
        public void Configure(EntityTypeBuilder<AdjustedItem> builder)
        {
            builder.ToTable("AdjustedItems");
            
            builder.HasKey(ai => ai.Id);
            
            builder.Property(ai => ai.Measurement)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(ai => ai.GramEquivalent)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            // Relationships
            builder.HasOne(ai => ai.Item)
                .WithMany()
                .HasForeignKey(ai => ai.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
                
            // Add index to prevent duplicate measurements for the same item
            builder.HasIndex(ai => new { ai.ItemId, ai.Measurement })
                .IsUnique();
        }
    }
}