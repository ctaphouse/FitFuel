using System.ComponentModel.DataAnnotations;

namespace FitFuel.Api.Models
{
    public class Gecko
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)] // Adjust max length as needed
        public string Name { get; set; } = string.Empty;
    }
}
