using System.ComponentModel.DataAnnotations;

namespace FitFuel.Shared.Dtos;

public class GroceryListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<GroceryListItemDto> Items { get; set; } = new List<GroceryListItemDto>();
}
