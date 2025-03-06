using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FitFuel.Shared.Dtos;

    public class AdjustedRecipeUpdateDto
    {
        [Required(ErrorMessage = "Measurement is required")]
        public string Measurement { get; set; } = string.Empty;
        
        [Range(1, 100, ErrorMessage = "Servings must be between 1 and 100")]
        public int Servings { get; set; }
    }
