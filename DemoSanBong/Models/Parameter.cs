using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class Parameter
    {
        [Key]
        public int Id { get; set; }
        public int DepositPercent { get; set; } // tỷ lệ đặt cọc
        public int OpenTime { get; set; }
        public int CloseTime { get; set; }
    }
}
