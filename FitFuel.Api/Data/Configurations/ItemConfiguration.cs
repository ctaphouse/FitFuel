using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Items");
            
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(i => i.Calories)
                .IsRequired();
                
            builder.Property(i => i.Protein)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            builder.Property(i => i.Carbohydrates)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            builder.Property(i => i.Fiber)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            builder.Property(i => i.Sugars)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            builder.Property(i => i.Fat)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            // Relationships
            builder.HasOne(i => i.ItemType)
                .WithMany()
                .HasForeignKey(i => i.ItemTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}