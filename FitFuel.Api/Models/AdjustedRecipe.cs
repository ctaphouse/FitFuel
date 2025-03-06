using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitFuel.Api.Models
{
    public class AdjustedRecipe
    {
        [Key]
        public int Id { get; set; }
        public int RecipeId { get; set; }

        public required string Measurement { get; set; }
        
        public int Servings { get; set; }
        
        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; } = null!;
    }
}
