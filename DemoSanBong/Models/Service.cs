﻿using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Unit { get; set; }
        [Required]
        public int Quantity { get; set; }

        public DateTime CreateDate { get; set; }
        public string? ImagePath { get; set; }

        private double currPrice;

        public void SetCurrPrice(AppDbContext context)
        {
            var f = context.ServiceRates.Where(i => i.ServiceId == Id && i.EffectiveDate < DateTime.Now).OrderByDescending(i => i.EffectiveDate).FirstOrDefault();
            if (f == null) currPrice= 0;
            currPrice= f.Price;
        }
        public double GetCurrPrice() { return currPrice; }



        public double getCurrentPrice(AppDbContext context) //Lấy giá giờ hiện tại
        {
            var f = context.ServiceRates.Where(i => i.ServiceId == Id && i.EffectiveDate < DateTime.Now).OrderByDescending(i => i.EffectiveDate).FirstOrDefault();
            if (f == null) return 0;
            return f.Price;
        }
        public double getPrice(AppDbContext context, DateTime date) //Lấy giá tại thời điểm
        {
            var f = context.ServiceRates.Where(i => i.ServiceId == Id && i.EffectiveDate < date).OrderByDescending(i => i.EffectiveDate).FirstOrDefault();
            if (f == null) return 0;
            return f.Price;
        }
    }
}
