using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FitFuel.Shared.Dtos;

    public class AdjustedItemDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        
        [Required(ErrorMessage = "Measurement is required")]
        public string Measurement { get; set; } = string.Empty;
        
        [Range(0.01, 10000, ErrorMessage = "Gram equivalent must be between 0.01 and 10000")]
        public decimal GramEquivalent { get; set; }
        
        // Navigation property representation
        public string? ItemName { get; set; }
    }
