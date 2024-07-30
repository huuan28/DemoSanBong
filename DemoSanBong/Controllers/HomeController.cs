using DemoSanBong.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace DemoSanBong.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var list = _context.FeedBacks.Where(i => i.IsShow == true).Include(a => a.Customer).ToList();
            var customer = await _userManager.GetUserAsync(HttpContext.User);
            if (customer != null)
            {
                ViewData["user"] = customer;
                var fb = list.FirstOrDefault(i => i.CusId == customer.Id);
                if (fb != null)
                    ViewData["feedback"] = fb;
                if (_context.Bookings.Any(i => i.CusID == customer.Id))
                    ViewBag.flag = true;
                else ViewBag.flag = false;
            }
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int stars, string comment)
        {
            if (ModelState.IsValid)
            {

                var customer = await _userManager.GetUserAsync(HttpContext.User);
                var feedbacks = _context.FeedBacks.FirstOrDefault(i => i.CusId == customer.Id);
                if (feedbacks == null)
                {
                    feedbacks = new FeedBack
                    {
                        Stars = stars,
                        Commment = comment,
                        CusId = customer.Id,
                        CreateDate = DateTime.Now,
                        IsShow = true
                    };
                    _context.FeedBacks.Add(feedbacks);
                }
                feedbacks.Commment = comment;
                feedbacks.Stars = stars;
                feedbacks.UpdateDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult HideFeedBack(string id)
        {
            var fb = _context.FeedBacks.Find(id);
            if (fb != null) fb.IsShow = false;
            _context.FeedBacks.Update(fb);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
