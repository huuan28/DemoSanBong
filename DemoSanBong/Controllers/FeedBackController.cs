using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
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
        public IActionResult Create ()
        {
            return View();
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
            var models = new List<EditFeedbackViewModel>();
            foreach (var fb in list)
            {
                var model = new EditFeedbackViewModel
                {
                    CusId = fb.CusId,
                    Stars = fb.Stars,
                    Commment = fb.Commment,
                    IsShow = fb.IsShow,
                };
                models.Add(model);
            }
            return View(models);
        }

    }
}
