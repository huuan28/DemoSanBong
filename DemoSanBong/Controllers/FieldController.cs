using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;


namespace DemoSanBong.Controllers
{
    public class FieldController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;
        public FieldController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        //trang list sân
        public IActionResult Index()
        {
            var field = _context.Fields.ToList();
            var model = new List<FieldViewModel>();
            foreach (var f in field)
            {
                var item = new FieldViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Type = f.Type,
                    Price = f.getCurrentPrice(_context),
                    PricePerMonth = f.GetCurrentPricePerMonth(_context),
                    Description = f.Description,
                    IsActive = f.IsActive,
                };
                var img = _context.FieldImages.FirstOrDefault(i => i.FieldId == f.Id && i.IsDefault == true);
                if (img != null)
                {
                    item.DefaultImage = f.ImagePath + img.FileName;
                }
                model.Add(item);
            }
            return View(model);
        }

        public IActionResult Create()
        {
            return View(new FieldViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(FieldViewModel model, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                //Kiểm tra trùng tên
                var field = _context.Fields.FirstOrDefault(i => i.Name == model.Name);
                if (field != null)
                {
                    ModelState.AddModelError(string.Empty, "Tên phòng đã tồn tại");
                    return View(field);
                }
                field = new Field
                {
                    Name = model.Name,
                    Description = model.Description,
                    Type = model.Type,
                    IsActive = model.IsActive,
                };
                _context.Fields.Add(field);
                await _context.SaveChangesAsync();

                //lưu path ảnh
                field.ImagePath = "Images/" + field.Id.ToString() + "/";
                _context.Fields.Update(field);
                await _context.SaveChangesAsync();
                //thêm giá sân
                _context.FieldRates.AddRange(
                    new FieldRate
                    {
                        FieldId = field.Id,
                        Price = model.Price,
                        Type = 0,
                        EffectiveDate = DateTime.Now
                    },
                    new FieldRate
                    {
                        FieldId = field.Id,
                        Price = model.PricePerMonth,
                        Type = 1,
                        EffectiveDate = DateTime.Now
                    });
                await _context.SaveChangesAsync();

                //Tạo folder sân
                string fieldFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", field.Id.ToString());
                if (!Directory.Exists(fieldFolder))
                {
                    Directory.CreateDirectory(fieldFolder);
                }
                //Lấy tên file
                string fileName = image.FileName;

                //lấy đường dẫn folder ảnh
                string filePath = Path.Combine(fieldFolder, fileName);

                //Lưu ảnh vào folder
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                //cập nhật đường dẫn ảnh sân


                var fieldImage = new FieldImage
                {
                    FieldId = field.Id,
                    IsDefault = true,
                    FileName = fileName,
                };
                _context.FieldImages.Add(fieldImage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "Hãy upload ảnh!");
            return View(model);
        }
        public IActionResult Detail(int id)
        {
            var field = _context.Fields.FirstOrDefault(f => f.Id == id);
            if (field == null)
                return NotFound();
            return View(field);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var field = _context.Fields.Find(id);
            if (field == null)
                return NotFound();
            _context.Fields.Remove(field);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var field = _context.Fields.Find(id);
            if (field == null)
                return NotFound();
            FieldViewModel model = new FieldViewModel
            {
                Id = field.Id,
                Name = field.Name,
                Description = field.Description,
                Type = field.Type,
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(FieldViewModel model)
        {
            var field = _context.Fields.FirstOrDefault(i => i.Id == model.Id);
            if (field == null)
                return NotFound();
            field.Name = model.Name;
            field.Description = model.Description;
            field.Type = model.Type;
            _context.Fields.Update(field);
            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", new { id = model.Id });
        }

    }
}