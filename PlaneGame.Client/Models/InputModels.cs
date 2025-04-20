using System.ComponentModel.DataAnnotations;

namespace PlaneGame.Client.Models
{
    public class DimentionsModel
    {
        [Required]
        [Range(5, 40, ErrorMessage = "Width must be between 5 and 40.")]
        public int Width { get; set; }
        [Required]
        [Range(5, 40, ErrorMessage = "Width must be between 5 and 40.")]
        public int Height { get; set; }
    }
}
