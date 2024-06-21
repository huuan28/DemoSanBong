using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoSanBong.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;
        public ServiceController(AppDbContext context)
        {
            _context = context;
        }
        //[Authorize(Roles ="Admin")]
        public IActionResult Index()
        {
            var list = _context.Services.ToList();
            var models = new List<EditServiceViewModel>();
            foreach (var sv in list)
            {
                var model = new EditServiceViewModel
                {
                    Id = sv.Id,
                    Name = sv.Name,
                    Description = sv.Description,
                    Type = sv.Type,
                    Price = sv.getCurrentPrice(_context),
                    Unit = sv.Unit,
                    Quantity = sv.Quantity,
                    CreateDate = DateTime.Now,
                };
                models.Add(model);
            }
            return View(models);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceViewModel service)
        {
            if(ModelState.IsValid)
            {
                var sv = _context.Services.FirstOrDefault(i => i.Name == service.Name);
                if (sv == null)
                {
                    sv = new Service
                    {
                        Name = service.Name,
                        Description = service.Description,
                        Type = service.Type,                      
                        Unit = service.Unit,
                        Quantity = service.Quantity,
                    };

                    _context.Services.Add(sv);
                    await _context.SaveChangesAsync();
                    var serviceRate = new ServiceRate
                    {
                        ServiceId = sv.Id,
                        EffectiveDate = DateTime.Now,
                        Price = service.Price
                    };
                    _context.ServiceRates.Add(serviceRate);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }             
            }
            return View();
        }
        public IActionResult Update(int id)
        {
            var sv = _context.Services.FirstOrDefault(i => i.Id == id);
            if (sv == null)
            {
                return NotFound();
            }
            var model = new EditServiceViewModel
            {
                Id = id,
                Name = sv.Name,
                Description = sv.Description,
                Type = sv.Type,
                Price = sv.getCurrentPrice(_context),
                Unit = sv.Unit,
                Quantity = sv.Quantity,
                CreateDate = DateTime.Now,
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Update(EditServiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sv = _context.Services.FirstOrDefault(i => i.Id == model.Id);
                if (sv == null)
                {
                    return NotFound();
                }
                sv.Name = model.Name;
                sv.Description = model.Description;
                sv.Type = model.Type;
                sv.Unit = model.Unit;
                sv.Quantity = model.Quantity;
                _context.Services.Update(sv);
                await _context.SaveChangesAsync();

                var currPrice = _context.ServiceRates.Where(i=>i.ServiceId==model.Id).OrderByDescending(i=>i.EffectiveDate).FirstOrDefault();
                if (currPrice.Price != model.Price)
                {
                    currPrice.Price = model.Price;
                    _context.ServiceRates.Update(currPrice);
                    await _context.SaveChangesAsync();
                }
                //chuyển hướng
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var sv =_context.Services.Find(id);
            if(sv == null)
            {
                return NotFound();
            }
            _context.Services.Remove(sv);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Details(int id)
        {
            var detail = _context.Services.FirstOrDefault(i => i.Id == id);
            if (detail == null)
            {
                return NotFound();
            }
            var details = new EditServiceViewModel
            {
                Name = detail.Name,
                Description = detail.Description,
                Type = detail.Type,
                Price = detail.getCurrentPrice(_context),
                Unit = detail.Unit,
                Quantity = detail.Quantity,
            };
            return View(details);
        }
    }
}
