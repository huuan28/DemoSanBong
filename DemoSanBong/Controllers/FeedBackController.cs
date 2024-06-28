using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using System.Security.Claims;

namespace DemoSanBong.Controllers
{
    [Authorize(Roles ="Customer")]
    public class FeedBackController : Controller
    {
        private readonly AppDbContext _Context;
        private readonly UserManager<AppUser> _userManager;

        public FeedBackController (AppDbContext context, UserManager<AppUser> userManager)
        {
            _Context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Create ()
        {
            var model = new CreateFeedbackViewModel();
            if(User.Identity.IsAuthenticated)
            {
                var user= await _userManager.GetUserAsync(HttpContext.User);
                model.CusId = user.Id;
            }
            return View(model);
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Create (CreateFeedbackViewModel model)
        {
            if (ModelState.IsValid)
            {
                var feedback = new FeedBack
                {
                    CusId = model.CusId,
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
            return View(model);           
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Update(EditFeedbackViewModel model)
        {
            if (ModelState.IsValid)
            {
                var fb = _Context.FeedBacks.FirstOrDefault(i => i.CusId == model.CusId);
                if (fb == null)
                {
                    return NotFound();
                }
                fb.Stars = model.Stars;
                fb.Commment = model.Commment;
                fb.UpdateDate = DateTime.Now;
                fb.IsShow = model.IsShow;
                _Context.FeedBacks.Update(fb);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Delete(string id)
        {
            var fb = _Context.FeedBacks.FirstOrDefault(i => i.CusId == id);
            if (fb == null)
            {
                return NotFound();
            }
            _Context.FeedBacks.Remove(fb);
            await _Context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Customer")]
        public IActionResult Details(string id)
        {
            var detail = _Context.FeedBacks.FirstOrDefault(i => i.CusId == id);
            if (detail == null)
            {
                return NotFound();
            }
            var details = new EditFeedbackViewModel()
            {
                CusId = detail.CusId,
                Stars = detail.Stars,
                Commment = detail.Commment,
                IsShow = detail.IsShow,
            };
            return View(details);
        }
        [Authorize (Roles ="Admin")]
        public async Task<IActionResult> Hide(string id)
        {
            var fb = _Context.FeedBacks.FirstOrDefault(i => i.CusId == id);
            if (fb == null)
            {
                return NotFound();
            }
            fb.IsShow = false;
            _Context.FeedBacks.Update(fb);
            await _Context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var list =_Context.FeedBacks.ToList();
            foreach( var item in list)
            {
                var user = _Context.Users.Find(item.CusId);
                item.Customer = user;
            }
            return View(list);
        }

    }
}
