using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoSanBong.Models
{
    public class FieldImage
    {
        public int FieldId { get; set; }
        public Field Field { get; set; }

        public string FileName { get; set; }
        public bool IsDefault { get; set; }
    }
}
