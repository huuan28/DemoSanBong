using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace DemoSanBong.Controllers
{
    public class BookingController :Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IServiceProvider _serviceProvider;

        private BookingViewModel currentBooking;

        private AppUser currentUser;

        public BookingController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ICompositeViewEngine viewEngine, IServiceProvider serviceProvider)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

            _viewEngine = viewEngine;
            _serviceProvider = serviceProvider;
        }
        private Task<AppUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);



        //kiểm tra sân trống
        public bool isAvailable(int roomId, DateTime startDate, DateTime endDate)
        {
            var bookings = _context.Bookings
                .Where(i =>
                    (i.CheckinDate <= startDate && i.CheckoutDate >= startDate) ||
                    (i.CheckinDate <= endDate && i.CheckoutDate >= endDate) ||
                    (i.CheckinDate >= startDate && i.CheckoutDate <= endDate))
                .ToList();
            foreach (var booking in bookings)
            {
                bool flag = _context.BookingDetails.Any(i => i.FieldId == roomId && i.BookingId == booking.Id);
                if (flag)
                    return false;
            }
            return true;
        }
        //cập nhật danh sách sân trống
        public void UpDateAvailbleField()
        {
            var fields = _context.Fields.ToList();
            currentBooking.AvailableField.Clear();
            foreach (var field in fields)
            {
                if (isAvailable(field.Id, currentBooking.CheckinDate, currentBooking.CheckoutDate))
                {
                    currentBooking.AvailableField.Add(
                    new FieldViewModel
                    {
                        Id = field.Id,
                        Name = field.Name,
                        Type = field.Type,
                        Price = field.getCurrentPrice(_context),
                        PricePerMonth = field.GetCurrentPricePerMonth(_context),
                        Description = field.Description
                    }
                    ); }
            }
            SaveBookingToSession(currentBooking);
        }
        public void RemoveFieldInvalid()
        {
            currentBooking = GetBookingFromSession();
            var fieldsToRemove = new List<FieldViewModel>();

            // Lặp qua các mục và thêm các mục không hợp lệ vào danh sách tạm thời
            foreach (var field in currentBooking.SelectedFields)
            {
                if (!isAvailable((int)field.Id, currentBooking.CheckinDate, currentBooking.CheckoutDate))
                {
                    fieldsToRemove.Add(field);
                }
            }

            // Xóa các mục không hợp lệ ra khỏi tập hợp gốc
            foreach (var field in fieldsToRemove)
            {
                currentBooking.SelectedFields.Remove(field);
            }
            SaveBookingToSession(currentBooking);
        }

        private BookingViewModel GetBookingFromSession()
        {
            var bookingJson = HttpContext.Session.GetString("CurrentBooking");
            BookingViewModel model;
            if (string.IsNullOrEmpty(bookingJson))
            {
                var rules = _context.Rules.FirstOrDefault();
                model = new BookingViewModel();
                model.AvailableField = new List<FieldViewModel>();
                model.SelectedFields = new List<FieldViewModel>();
                var Times = new BookingTime(rules);
                model.SelectDay = Times.GetDays();
                model.SelectBegin = Times.SelectBegin(DateTime.Today);
                model.SelectEnd = Times.SelectEnd(model.SelectBegin[0]);
                model.CheckinDate = model.SelectBegin.FirstOrDefault();
                model.CheckoutDate = model.SelectEnd.FirstOrDefault();
                return model;
            }
            model = JsonConvert.DeserializeObject<BookingViewModel>(bookingJson);
            return model;
        }
        //Lưu thông tin đặt sân vào session
        private void SaveBookingToSession(BookingViewModel booking)
        {
            var bookingJson = JsonConvert.SerializeObject(booking);
            HttpContext.Session.SetString("CurrentBooking", bookingJson);
        }


        public async Task<IActionResult> Booking()
        {
            currentBooking = GetBookingFromSession();
            currentUser = await GetCurrentUserAsync();
            if(currentUser != null)
            {
                currentBooking.FullName = currentUser.UserName;
                currentBooking.PhoneNumber = currentUser.PhoneNumber;
            }
           
            UpDateAvailbleField();

            SaveBookingToSession(currentBooking);
            return View(currentBooking);
        }

        public IActionResult AddField(int Id)
        {
            currentBooking = GetBookingFromSession();
            var field = _context.Fields.Find(Id);
            if (!currentBooking.SelectedFields.Any(i => i.Id == Id))
                currentBooking.SelectedFields.Add( new FieldViewModel
                {
                    Id = field.Id,
                    Name = field.Name,
                    Type = field.Type,
                    Price = field.getCurrentPrice(_context),
                    PricePerMonth = field.GetCurrentPricePerMonth(_context),
                    Description = field.Description
                });
            SaveBookingToSession(currentBooking);
            return PartialView("SelectedField", currentBooking.SelectedFields);
        }
        public IActionResult RemoveField(int Id)
        {
            currentBooking = GetBookingFromSession();
            var field = currentBooking.SelectedFields.FirstOrDefault(i=>i.Id==Id);
            if (field != null)
                currentBooking.SelectedFields.Remove(field);
            SaveBookingToSession(currentBooking);
            return PartialView("SelectedField", currentBooking.SelectedFields);
        }
        //[HttpPost]
        //public IActionResult UpdateTime(DateTime start, DateTime end)
        //{
        //    currentBooking = GetBookingFromSession();
        //    currentBooking.CheckinDate = start;
        //    currentBooking.CheckoutDate = end;
        //    var rules = _context.Rules.FirstOrDefault();
        //    currentBooking.SelectEnd = new BookingTime(rules).SelectEnd(start);
        //    UpDateAvailbleField();
        //    RemoveFieldInvalid();
        //    SaveBookingToSession(currentBooking);
        //    //return PartialView("AvailableField", currentBooking.AvailableField);
        //    return View("Booking", currentBooking);
        //}
        [HttpPost]
        public IActionResult UpdateTime(DateTime start, DateTime end)
        {
            currentBooking = GetBookingFromSession();
            currentBooking.CheckinDate = start;
            currentBooking.CheckoutDate = end;
            var rules = _context.Rules.FirstOrDefault();
            currentBooking.SelectEnd = new BookingTime(rules).SelectEnd(start);
            UpDateAvailbleField();
            RemoveFieldInvalid();
            SaveBookingToSession(currentBooking);

            var availableFieldsHtml = RenderPartialViewToString("AvailableField", currentBooking.AvailableField);
            var selectedFieldsHtml = RenderPartialViewToString("SelectedField", currentBooking.SelectedFields);

            return Json(new { availableFieldsHtml, selectedFieldsHtml });
        }

        private string RenderPartialViewToString(string viewName, object model)
        {
            var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.Success == false)
            {
                throw new InvalidOperationException($"Unable to find view '{viewName}'");
            }

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            using (var writer = new StringWriter())
            {
                var tempDataProvider = HttpContext.RequestServices.GetService(typeof(ITempDataProvider)) as ITempDataProvider;
                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(ControllerContext.HttpContext, tempDataProvider),
                    writer,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return writer.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        public IActionResult UpdateBeginEndTimes(DateTime selectedDate)
        {
            var rules = _context.Rules.FirstOrDefault();
            var times = new BookingTime(rules);

            var selectBegin = times.SelectBegin(selectedDate);
            var selectEnd = times.SelectEnd(selectBegin.FirstOrDefault());

            currentBooking = GetBookingFromSession();
            currentBooking.CheckinDate = selectBegin.FirstOrDefault();
            currentBooking.CheckoutDate = selectEnd.FirstOrDefault();
            UpDateAvailbleField();
            RemoveFieldInvalid();
            SaveBookingToSession(currentBooking);
            var result = new
            {
                SelectBegin = selectBegin.Select(d => d.ToString("yyyy-MM-dd HH:mm")).ToList(),
                SelectEnd = selectEnd.Select(d => d.ToString("yyyy-MM-dd HH:mm")).ToList()
            };

            return Json(result);
        }
    }
}
