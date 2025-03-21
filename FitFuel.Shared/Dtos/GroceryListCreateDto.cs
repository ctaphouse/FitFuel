using System.ComponentModel.DataAnnotations;

namespace FitFuel.Shared.Dtos;

public class GroceryListCreateDto
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;
    
    public List<int> RecipeIds { get; set; } = new List<int>();
    public List<IndividualItemDto> IndividualItems { get; set; } = new List<IndividualItemDto>();
}
