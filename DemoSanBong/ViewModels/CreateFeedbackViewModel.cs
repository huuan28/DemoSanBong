using DemoSanBong.Models;

namespace DemoSanBong.ViewModels
{
    public class CreateFeedbackViewModel
    {
        public string CusId { get; set; }
        public int Stars { get; set; }
        public string Commment { get; set; }
    }
    public class EditFeedbackViewModel
    {
        public string CusId { get; set; }
        public int Stars { get; set; }
        public string Commment { get; set; }
        public bool IsShow { get; set; }
    }
}
