// Add this validation to the ItemCreateDto class in FitFuel.Shared/Dtos/ItemCreateDto.cs:

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FitFuel.Shared.Dtos;

public class ItemCreateDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Range(0, 9999999, ErrorMessage = "Calories must be between 0 and 9,999,999")]
    public int Calories { get; set; }
    
    [Range(0, 99999999.99, ErrorMessage = "Protein must be between 0 and 99,999,999.99")]
    public decimal Protein { get; set; }
    
    [Range(0, 99999999.99, ErrorMessage = "Carbohydrates must be between 0 and 99,999,999.99")]
    public decimal Carbohydrates { get; set; }
    
    [Range(0, 99999999.99, ErrorMessage = "Fiber must be between 0 and 99,999,999.99")]
    public decimal Fiber { get; set; }
    
    [Range(0, 99999999.99, ErrorMessage = "Sugars must be between 0 and 99,999,999.99")]
    public decimal Sugars { get; set; }
    
    [Range(0, 99999999.99, ErrorMessage = "Fat must be between 0 and 99,999,999.99")]
    public decimal Fat { get; set; }
    
    [Required(ErrorMessage = "Item Type is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an item type")]
    public int ItemTypeId { get; set; }
}