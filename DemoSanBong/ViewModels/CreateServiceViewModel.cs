using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class CreateServiceViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }
        public string Type { get; set; }
        public double Price { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public string? ImagePath { get; set; }
    }
}
