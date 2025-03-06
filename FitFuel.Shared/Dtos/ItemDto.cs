using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FitFuel.Shared.Dtos;

    public class ItemDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;
        
        public int Calories { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Protein must be between 0 and 1000")]
        public decimal Protein { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Carbohydrates must be between 0 and 1000")]
        public decimal Carbohydrates { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Fiber must be between 0 and 1000")]
        public decimal Fiber { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Sugars must be between 0 and 1000")]
        public decimal Sugars { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Fat must be between 0 and 1000")]
        public decimal Fat { get; set; }
        
        public int ItemTypeId { get; set; }
        
        // Navigation property representation
        public string? ItemTypeName { get; set; }
    }
