using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitFuel.Api.Models;

namespace FitFuel.Api.Data.Configurations;

    public class GroceryListConfiguration : IEntityTypeConfiguration<GroceryList>
    {
        public void Configure(EntityTypeBuilder<GroceryList> builder)
        {
            builder.ToTable("GroceryLists");
            
            builder.HasKey(gl => gl.Id);
            
            builder.Property(gl => gl.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(gl => gl.CreatedDate)
                .IsRequired();
        }
    }
