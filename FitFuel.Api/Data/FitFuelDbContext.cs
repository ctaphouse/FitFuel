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

        // Add your DbSet properties here


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize entity mappings here if needed
        }
    }
}
