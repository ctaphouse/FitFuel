using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data.Configurations;

    public class GroceryListItemConfiguration : IEntityTypeConfiguration<GroceryListItem>
    {
        public void Configure(EntityTypeBuilder<GroceryListItem> builder)
        {
            builder.ToTable("GroceryListItems");
            
            builder.HasKey(gli => gli.Id);
            
            builder.Property(gli => gli.Measurement)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(gli => gli.Quantity)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
                
            builder.Property(gli => gli.IsChecked)
                .IsRequired();
                
            builder.Property(gli => gli.Source)
                .HasMaxLength(20);
                
            // Relationships
            builder.HasOne(gli => gli.GroceryList)
                .WithMany()
                .HasForeignKey(gli => gli.GroceryListId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
                
            builder.HasOne(gli => gli.Item)
                .WithMany()
                .HasForeignKey(gli => gli.ItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
