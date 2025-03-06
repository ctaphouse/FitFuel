using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitFuel.Api.Models
{
    public class RecipeItem
    {
        [Key]
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int ItemId { get; set; }
        
        public required string Measurement { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal GramEquivalent { get; set; }
        
        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; } = null!;
        
        [ForeignKey("ItemId")]
        public Item Item { get; set; } = null!;
    }
}
