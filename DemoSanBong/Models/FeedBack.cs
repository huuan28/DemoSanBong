namespace DemoSanBong.Models
{
    public class FeedBack
    {
        public string CusId { get; set; }
        public AppUser Customer { get; set; }
        public int Stars { get; set; }
        public string Commment { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public bool IsShow { get; set; }
    }
}
