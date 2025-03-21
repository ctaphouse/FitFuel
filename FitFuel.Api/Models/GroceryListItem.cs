using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitFuel.Api.Models;

    public class GroceryListItem
    {
        [Key]
        public int Id { get; set; }
        public int GroceryListId { get; set; }
        public int ItemId { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }
        public required string Measurement { get; set; }
        public bool IsChecked { get; set; } = false;
        public string? Source { get; set; } // "Recipe" or "Individual"
        public int? SourceId { get; set; } // RecipeId if Source is "Recipe"
        
        [ForeignKey("GroceryListId")]
        public GroceryList GroceryList { get; set; } = null!;
        
        [ForeignKey("ItemId")]
        public Item Item { get; set; } = null!;
    }
