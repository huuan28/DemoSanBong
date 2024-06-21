using NuGet.Protocol;
using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class Field
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Mã sân")]
        public string Name { get; set; }

        [Required,Display(Name ="Loại sân")]
        public string Type { get; set; }

        [Display(Name = "Mô tả sân")]
        public string? Description { get; set; }

        public bool IsActive { get; set; }//hoạt động/bảo trì

        public string? ImagePath { get; set; }

        public double getCurrentPrice(AppDbContext context) //Lấy giá giờ hiện tại
        {
            var f = context.FieldRates.Where(i=>i.FieldId == Id&&i.EffectiveDate<DateTime.Now&&i.Type==0).OrderByDescending(i=>i.EffectiveDate).FirstOrDefault();
            if (f == null) return 0;
            return f.Price;
        }
        public double GetCurrentPricePerMonth(AppDbContext context)// giá tháng hiện tại
        {
            var f = context.FieldRates.Where(i => i.FieldId == Id && i.EffectiveDate < DateTime.Now && i.Type == 1).OrderByDescending(i => i.EffectiveDate).FirstOrDefault();
            if (f == null) return 0;
            return f.Price;
        }
        public double GetPrice(AppDbContext context, DateTime date)// tra giá tại một thời điểm
        {
            var f = context.FieldRates.Where(i => i.FieldId == Id && i.EffectiveDate < date && i.Type == 0).OrderByDescending(i => i.EffectiveDate).FirstOrDefault();
            if (f == null) return 0;
            return f.Price;
        }
        public double GetPricePerMonth(AppDbContext context, DateTime date) //tra giá tháng thại thời điểm
        {
            var f = context.FieldRates.Where(i => i.FieldId == Id && i.EffectiveDate < date && i.Type == 1).OrderByDescending(i => i.EffectiveDate).FirstOrDefault();
            if (f == null) return 0;
            return f.Price;
        }

        public List<FieldImage>? images(AppDbContext context)
        {
            var list = context.FieldImages.Where(i => i.FieldId == Id).ToList();
            return list;
        }
    }
}
