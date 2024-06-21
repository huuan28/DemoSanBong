using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoSanBong.Models
{
    public class BookingDetail
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int FieldId { get; set; }
        public Field Field { get; set; }

    }
}
