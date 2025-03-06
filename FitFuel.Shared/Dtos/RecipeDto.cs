using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FitFuel.Shared.Dtos;

    public class RecipeDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;
        
        [Range(1, 100, ErrorMessage = "Servings must be between 1 and 100")]
        public int Servings { get; set; }
        
        // Calculated nutritional properties
        public int TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbohydrates { get; set; }
        public decimal TotalFiber { get; set; }
        public decimal TotalSugars { get; set; }
        public decimal TotalFat { get; set; }
        
        // Per serving nutritional properties
        public int CaloriesPerServing { get; set; }
        public decimal ProteinPerServing { get; set; }
        public decimal CarbohydratesPerServing { get; set; }
        public decimal FiberPerServing { get; set; }
        public decimal SugarsPerServing { get; set; }
        public decimal FatPerServing { get; set; }
    }
