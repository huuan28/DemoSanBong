using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class CreateServiceViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public double Unit { get; set; }
        [Required]
        public int Quantity { get; set; }

        public string? ImagePath { get; set; }
    }
}
