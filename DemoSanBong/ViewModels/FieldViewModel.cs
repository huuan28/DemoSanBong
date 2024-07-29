using DemoSanBong.Models;
using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.ViewModels
{
    public class FieldViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Mã sân")]
        public string Name { get; set; }

        [Display(Name = "Mô tả sân")]
        public string Description { get; set; }

        [Required, Display(Name = "Loại sân")]
        public string Type { get; set; }

        [Display(Name = "Giá giờ")]
        public double Price { get; set; }

        [Display(Name = "Giá tháng")]
        public double PricePerMonth { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; }

        [Display(Name = "Ảnh mặc định")]
        public string? DefaultImage { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        private List<string> images;

        public void SetImages(List<string> images)
        {
            this.images = images;
        }
        public List<string> GetImages() { return images; }
    }
}
