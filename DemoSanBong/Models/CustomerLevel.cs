using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class CustomerLevel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Benefit { get; set; }
    }
}
