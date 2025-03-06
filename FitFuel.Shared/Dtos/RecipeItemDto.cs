using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FitFuel.Shared.Dtos;

    public class RecipeItemDto
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int ItemId { get; set; }
        
        [Required(ErrorMessage = "Measurement is required")]
        public string Measurement { get; set; } = string.Empty;
        
        [Range(0.01, 10000, ErrorMessage = "Gram equivalent must be between 0.01 and 10000")]
        public decimal GramEquivalent { get; set; }
        
        // Navigation property representations
        public string? RecipeName { get; set; }
        public string? ItemName { get; set; }
        
        // Calculated nutritional properties for this recipe item
        public int Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbohydrates { get; set; }
        public decimal Fiber { get; set; }
        public decimal Sugars { get; set; }
        public decimal Fat { get; set; }
    }
