using DemoSanBong.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoSanBong.Controllers
{
    [Authorize(Roles ="Customer")]
    public class FeedBackController : Controller
    {
        private readonly AppDbContext _Context;

        public FeedBackController (AppDbContext context)
        {
            _Context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create (FeedBack model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(_Context.FeedBacks.Any(f => f.CusId == userId))
            {
                return BadRequest("Bạn đã để lại feed back");
            }

            var feedback = new FeedBack { 
                CusId = userId,
                Stars = model.Stars,
                Commment = model.Commment,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                IsShow = true
            };
            _Context.FeedBacks.Add(feedback);
            await _Context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
