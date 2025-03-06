using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FitFuel.Shared.Dtos;

    public class NutritionCalculationDto
    {
        public int Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbohydrates { get; set; }
        public decimal Fiber { get; set; }
        public decimal Sugars { get; set; }
        public decimal Fat { get; set; }
    }
