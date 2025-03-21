using System.ComponentModel.DataAnnotations;

namespace FitFuel.Shared.Dtos;

public class IndividualItemDto
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string Measurement { get; set; } = string.Empty;
}
