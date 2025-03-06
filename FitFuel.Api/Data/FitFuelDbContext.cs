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
         public DbSet<Gecko> Geckos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize entity mappings here if needed
        }
    }
}
