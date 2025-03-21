using System.ComponentModel.DataAnnotations;

namespace FitFuel.Shared.Dtos;

public class GroceryListItemDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int ItemTypeId { get; set; }
    public string ItemTypeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Measurement { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public string? Source { get; set; } // "Recipe" or "Individual"
    public int? SourceId { get; set; } // RecipeId if Source is "Recipe"
    public string? SourceName { get; set; } // Recipe name if Source is "Recipe"
}
