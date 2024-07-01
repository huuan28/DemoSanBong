using Microsoft.AspNetCore.Mvc;

namespace DemoSanBong.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
