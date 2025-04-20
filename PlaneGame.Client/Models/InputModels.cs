using System.ComponentModel.DataAnnotations;

namespace PlaneGame.Client.Models
{
    public class DimentionsModel
    {
        [Required]
        [Range(5, 1000, ErrorMessage = "Width must be between 1 and 1000.")]
        public int Width { get; set; }
        [Required]
        [Range(5, 1000, ErrorMessage = "Width must be between 1 and 1000.")]
        public int Height { get; set; }
    }
}
