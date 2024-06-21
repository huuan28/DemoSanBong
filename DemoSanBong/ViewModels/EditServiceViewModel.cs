using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class EditServiceViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public string Unit { get; set; }
        [Required]
        public int Quantity { get; set; }

        public DateTime CreateDate { get; set; }
        public string? ImagePath { get; set; }
    }
}
