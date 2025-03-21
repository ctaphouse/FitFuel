using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitFuel.Api.Models;

    public class GroceryList
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
