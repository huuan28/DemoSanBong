using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DemoSanBong.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;
        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var model = new StatisticView()
            {
                Revenue = new double[12]
            };

            for (int i = 0; i < 12; i++)
            {
                double x;
                x = _context.Invoices.Where(a=>a.CheckoutDate.Value.Year==DateTime.Now.Year&&a.CheckoutDate.Value.Month==(i+1)).Sum(i => i.Amount);
                model.Revenue[i] = x;
            }
            return View(model);
        }
    }
}
