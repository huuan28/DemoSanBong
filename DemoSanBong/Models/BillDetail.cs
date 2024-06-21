namespace DemoSanBong.Models
{
    public class BillDetail
    {
        public int GuestBillId { get; set; }
        public GuestBill GuestBill { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int Quantity { get; set; }
    }
}
