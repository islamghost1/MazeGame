using System.ComponentModel.DataAnnotations;

namespace PlaneGame.Client.Models
{
    public class DimentionsModel
    {
        [Required]
        [Range(5, 100, ErrorMessage = "Width must be between 5 and 100.")]
        public int Width { get; set; }
        [Required]
        [Range(5, 100, ErrorMessage = "Height must be between 5 and 100.")]
        public int Height { get; set; }
    }
}
