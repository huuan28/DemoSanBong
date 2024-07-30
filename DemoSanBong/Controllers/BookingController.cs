using DemoSanBong.Models;
using DemoSanBong.Services;
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
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly IVnPayService _vnPayService;

        private BookingViewModel currentBooking;

        private AppUser currentUser;

        public BookingController(AppDbContext context, UserManager<AppUser> userManager, ICompositeViewEngine viewEngine, IServiceProvider serviceProvider, IVnPayService vnPayService)
        {
            _context = context;
            _userManager = userManager;

            _viewEngine = viewEngine;
            _serviceProvider = serviceProvider;
            _vnPayService = vnPayService;
        }

        //Lấy user đang đăng nhập
        private Task<AppUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);



        //kiểm tra sân trống
        private bool IsAvailable(int fieldId, DateTime startDate, DateTime endDate)
        {
            ////tìm lịch đặt sân có cùng thời gian chọn
            //var bookings = _context.Bookings
            //    .Where(i =>
            //        (i.CheckinDate <= startDate && i.CheckoutDate >= startDate) ||
            //        (i.CheckinDate <= endDate && i.CheckoutDate >= endDate) ||
            //        (i.CheckinDate >= startDate && i.CheckoutDate <= endDate))
            //    .ToList();

            ////Duyệt qua các sân có trong danh sách lịch đặt vừa tìm để lọc ra những sân đã đặt
            //foreach (var booking in bookings)
            //{
            //    bool flag = _context.BookingDetails.Any(i => i.FieldId == roomId && i.BookingId == booking.Id);
            //    if (flag)
            //        return false;
            //}

            var bkdts = _context.BookingDetails.Where(i =>
                     (i.StartTime <= startDate && i.EndTime >= startDate) ||
                     (i.StartTime <= endDate && i.EndTime >= endDate) ||
                     (i.StartTime >= startDate && i.EndTime <= endDate))
                .ToList();

            if (bkdts.Any(i => i.FieldId == fieldId))
                return false;
            return true;
        }

        //cập nhật danh sách sân trống
        public void UpDateAvailbleField()
        {
            var fields = _context.Fields.ToList();
            currentBooking.AvailableField.Clear();
            foreach (var field in fields)
            {
                if (IsAvailable(field.Id, (DateTime)currentBooking.CheckinDate, (DateTime)currentBooking.CheckoutDate))
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
                    );
                }
            }
            SaveBookingToSession(currentBooking);
        }
        public List<FieldViewModel> GetAvailbleField(BookingTime time)
        {
            var list = new List<FieldViewModel>();
            var fields = _context.Fields.ToList();
            var begin = time.SelectBegin(time.GetDays().FirstOrDefault()).FirstOrDefault();
            foreach (var field in fields)
            {
                if (IsAvailable(field.Id, begin, begin.AddHours(1)))
                {
                    list.Add(
                    new FieldViewModel
                    {
                        Id = field.Id,
                        Name = field.Name,
                        Type = field.Type,
                        Price = field.getCurrentPrice(_context),
                        PricePerMonth = field.GetCurrentPricePerMonth(_context),
                        Description = field.Description
                    });
                }
            }
            return list;
        }
        //bỏ sân vi phạm ràng buộc thời gian
        public void RemoveFieldInvalid()
        {
            currentBooking = GetBookingFromSession();
            var fieldsToRemove = new List<FieldViewModel>();

            // Lặp qua các mục và thêm các mục không hợp lệ vào danh sách tạm thời
            foreach (var field in currentBooking.SelectedFields.SelectedFields)
            {
                if (!IsAvailable((int)field.Id, (DateTime)currentBooking.CheckinDate, (DateTime)currentBooking.CheckoutDate))
                {
                    fieldsToRemove.Add(field);
                }
            }

            // Xóa các mục không hợp lệ ra khỏi tập hợp gốc
            foreach (var field in fieldsToRemove)
            {
                currentBooking.SelectedFields.SelectedFields.Remove(field);
            }
            SaveBookingToSession(currentBooking);
        }

        //lấy thông tin đặt sân từ sesstion
        private BookingViewModel GetBookingFromSession()
        {
            var bookingJson = HttpContext.Session.GetString("CurrentBooking");
            BookingViewModel model;
            if (string.IsNullOrEmpty(bookingJson))
            {
                var rules = _context.Parameters.FirstOrDefault();
                var Times = new BookingTime(rules);
                model = new BookingViewModel();
                model.AvailableField = GetAvailbleField(Times);
                model.SelectedFields = new SelectedFieldsViewModel();
                model.SelectedFields.SelectedFields = new List<FieldViewModel>();
                model.SelectDay = Times.GetDays();
                model.SelectedDate = DateTime.Today;
                model.SelectBegin = Times.SelectBegin(model.SelectDay.FirstOrDefault());
                model.SelectEnd = Times.SelectEnd(model.SelectBegin.FirstOrDefault());
                model.CheckinDate = model.SelectBegin.FirstOrDefault();
                model.CheckoutDate = model.SelectEnd.FirstOrDefault();
                model.MonthNum = 1;
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

        //thêm sân vào danh sách chọn
        public IActionResult AddField(int Id)
        {
            currentBooking = GetBookingFromSession();
            var field = _context.Fields.Find(Id);
            if (!currentBooking.SelectedFields.SelectedFields.Any(i => i.Id == Id))
            {
                var f = new FieldViewModel
                {
                    Id = field.Id,
                    Name = field.Name,
                    Type = field.Type,
                    Price = field.getCurrentPrice(_context),
                    PricePerMonth = field.GetCurrentPricePerMonth(_context),
                    Description = field.Description,
                    Start = currentBooking.CheckinDate,
                    End = currentBooking.CheckoutDate,
                };
                currentBooking.SelectedFields.SelectedFields.Add(f);
                if (currentBooking.RentalType == 0)
                    currentBooking.SelectedFields.Amount += f.Price * (f.End.Value.Hour - f.Start.Value.Hour);
                else
                {
                    int dif = CalculateMonthsDifference((DateTime)f.Start, (DateTime)f.End);
                    currentBooking.SelectedFields.Amount += f.PricePerMonth * dif;
                }
                currentBooking.SelectedFields.Deposit = currentBooking.SelectedFields.Amount * 0.2;
                currentBooking.Deposit = currentBooking.SelectedFields.Amount * 0.2;
            }
            SaveBookingToSession(currentBooking);
            return PartialView("SelectedField", currentBooking.SelectedFields);
        }

        //bỏ sân khỏi danh sách chọn
        public IActionResult RemoveField(int Id)
        {
            currentBooking = GetBookingFromSession();
            var f = currentBooking.SelectedFields.SelectedFields.FirstOrDefault(i => i.Id == Id);
            if (f != null)
                currentBooking.SelectedFields.SelectedFields.Remove(f);
            if (currentBooking.RentalType == 0)
                currentBooking.SelectedFields.Amount -= f.Price * (f.End.Value.Hour - f.Start.Value.Hour);
            else
            {
                int dif = CalculateMonthsDifference((DateTime)f.Start, (DateTime)f.End);
                currentBooking.SelectedFields.Amount -= f.PricePerMonth *dif;
            }
            currentBooking.SelectedFields.Deposit = currentBooking.SelectedFields.Amount * 0.2;
            currentBooking.Deposit = currentBooking.SelectedFields.Amount * 0.2;
            SaveBookingToSession(currentBooking);
            return PartialView("SelectedField", currentBooking.SelectedFields);
        }

        //Cập nhật sân trống khi thay đổi option giờ trong form đặt sân
        [HttpPost]
        public IActionResult UpdateTime(DateTime start, DateTime end)
        {
            currentBooking = GetBookingFromSession();
            currentBooking.CheckinDate = start;
            currentBooking.CheckoutDate = end;
            var rules = _context.Parameters.FirstOrDefault();
            currentBooking.SelectEnd = new BookingTime(rules).SelectEnd(start);
            UpDateAvailbleField();
            //RemoveFieldInvalid();
            SaveBookingToSession(currentBooking);

            var availableFieldsHtml = RenderPartialViewToString("AvailableField", currentBooking.AvailableField);
            var selectedFieldsHtml = RenderPartialViewToString("SelectedField", currentBooking.SelectedFields);
            //Chuỗi JSON trả về view cho AJAX xử lý cập nhật 2 partialview để không tải lại trang
            return Json(new { availableFieldsHtml, selectedFieldsHtml });
        }


        //chuyển dữ liệu của partial view sang dạng chuỗi JSON
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


        //Cập nhật lại 2 thẻ select thời gian bắt đầu và kết thúc dựa vào ngày được chọn
        //nếu là hôm nay thì phải bắt đầu từ giờ hiện tại +1 (vD: hiện tại 9h15 thì option đầu tiên là 10h
        //ngược lại thì options từ 6-21(begin) | 7-22(end)
        [HttpPost]
        public IActionResult UpdateSelectedDay(DateTime selectedDate)
        {
            //lấy dữ liệu từ bảng tham số quy định để có giờ mở sân và giờ đóng sân
            var rules = _context.Parameters.FirstOrDefault(); //lấy các tham số qui định 
            var times = new BookingTime(rules); //BookingTime là lớp chức năng chứa các phương thức lấy thời gian đặt sân

            var selectBegin = times.SelectBegin(selectedDate); //danh sách option giờ bắt đầu
            var selectEnd = times.SelectEnd(selectBegin.FirstOrDefault()); //danh sách option giờ kết thúc

            currentBooking = GetBookingFromSession();//lấy dữ liệu của form hiện tại đang lưu trong session

            if (currentBooking.RentalType == 0)
            {
                currentBooking.CheckinDate = selectBegin.FirstOrDefault();
                currentBooking.CheckoutDate = selectEnd.FirstOrDefault();
            }
            else
            {
                currentBooking.CheckinDate = selectedDate.Date;
                currentBooking.CheckoutDate = selectedDate.Date.AddMonths((int)currentBooking.MonthNum);
            }
            currentBooking.SelectedDate = selectedDate;
            UpDateAvailbleField(); //cập nhật sân trống
            RemoveFieldInvalid(); //bỏ sân đã chọn bị nhưng không còn trống với giờ vừa thay đổi
            SaveBookingToSession(currentBooking); //lưu thông tin đã chọn vào session
            var result = new
            {
                SelectBegin = selectBegin.Select(d => d.ToString("yyyy-MM-dd HH:mm")).ToList(),
                SelectEnd = selectEnd.Select(d => d.ToString("yyyy-MM-dd HH:mm")).ToList()
            };

            return Json(result);
        }

        [HttpPost]
        public IActionResult UpdateRentalType(int rentalType)
        {
            currentBooking = GetBookingFromSession();
            currentBooking.RentalType = rentalType;
            currentBooking.SelectedFields.RentalType = rentalType;
            UpDateAvailbleField();
            //RemoveFieldInvalid();
            currentBooking.SelectedFields.SelectedFields.Clear();
            currentBooking.SelectedFields.Amount = 0;
            currentBooking.SelectedFields.Deposit = 0;
            SaveBookingToSession(currentBooking);

            return PartialView("SelectedField", currentBooking.SelectedFields);
        }

        [HttpPost]
        public IActionResult UpdateMonthNum(int MonthNum)
        {
            currentBooking = GetBookingFromSession();
            currentBooking.MonthNum = MonthNum;
            currentBooking.SelectedFields.MonthNum = MonthNum;
            currentBooking.CheckinDate = currentBooking.SelectedDate;
            currentBooking.CheckoutDate = currentBooking.SelectedDate.AddMonths(MonthNum);
            UpDateAvailbleField();
            currentBooking.SelectedFields.SelectedFields.Clear();
            currentBooking.SelectedFields.Amount = 0;
            currentBooking.SelectedFields.Deposit = 0;
            SaveBookingToSession(currentBooking);

            return PartialView("SelectedField", currentBooking.SelectedFields);
        }

        //View đặt sân
        public async Task<IActionResult> Booking()
        {
            currentBooking = GetBookingFromSession();
            currentUser = await GetCurrentUserAsync();
            if (currentUser != null)
            {
                currentBooking.FullName = currentUser.UserName;
                currentBooking.PhoneNumber = currentUser.PhoneNumber;
            }

            //UpDateAvailbleField();

            SaveBookingToSession(currentBooking);
            return View(currentBooking);
        }

        [HttpPost]
        public async Task<IActionResult> Booking(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(i => i.PhoneNumber == model.PhoneNumber);
                //Kiểm tra đã đăng ký chưa
                if (user == null)
                {
                    if (!await CreateUnRegisterUser(model.PhoneNumber, model.FullName)) ; //lưu tài khoản loại chưa đăng ký
                    user = await _userManager.FindByNameAsync(model.PhoneNumber);
                }
                else if (user.FullName != model.FullName && user.IsRegisted)
                {
                    ViewBag.Error = "SĐT này đã sử dụng với tên khác, hãy đăng nhập để cập nhật họ và tên!";
                    return View(model);
                }
                var rules = _context.Parameters.FirstOrDefault();

                currentBooking = GetBookingFromSession();
                currentBooking.FullName = model.FullName;
                currentBooking.PhoneNumber = model.PhoneNumber;
                currentBooking.RentalType = model.RentalType;

                SaveBookingToSession(currentBooking);
                var vnPayModel = new VnPaymentRequestModel()
                {
                    Amount = (double)currentBooking.Deposit,
                    CreateDate = DateTime.Now,
                    Description = $"{model.PhoneNumber}-{model.FullName}",
                    FullName = model.FullName,
                    BookingId = _context.Bookings.Max(i => i.Id)
                };
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel, "-"));
            }
            return RedirectToAction("Booking");
        }

        //Tạo tài khoản tạm cho số điện thoại chưa đăng ký
        public async Task<bool> CreateUnRegisterUser(string Phone, string FullName)
        {
            if (_context.Users.Any(i => i.PhoneNumber == Phone))
                return false;

            var user = new AppUser
            {
                UserName = Phone,
                FullName = FullName,
                IsRegisted = false,
                PhoneNumber = Phone,
                CreateDate = DateTime.Now,
            };
            var result = await _userManager.CreateAsync(user, "Abcd@1234");
            if (result.Succeeded)
            {
                // Gán vai trò "Customer" cho người dùng
                await _userManager.AddToRoleAsync(user, "Customer");

                return true;
            }
            return false;
        }

        //kết quả trả về của VNPAY
        public async Task<IActionResult> PaymentCallBack()
        {
            // Lấy thông tin từ query string của VnPay để xác thực và cập nhật trạng thái đơn hàng
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                //thanh toán thất bại
                return RedirectToAction("PaymentFail");
            }

            //lấy thông tin đặt phòng
            currentBooking = GetBookingFromSession();

            //lấy thông tin khách hàng
            var user = _context.Users.FirstOrDefault(i => i.PhoneNumber == currentBooking.PhoneNumber);
            // tạo mới đơn đặt phòng
            var booking = new Booking
            {
                CreateDate = DateTime.Now,
                //CheckinDate = currentBooking.CheckinDate,
                //CheckoutDate = currentBooking.CheckoutDate,
                Deposit = (double)currentBooking.Deposit,
                CusID = user.Id,
                Status = Models.Booking.Deposited,
                RentalType = currentBooking.RentalType
            };
            //lưu vào DB
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            //thêm và lưu danh sách phòng đã chọn
            foreach (var field in currentBooking.SelectedFields.SelectedFields)
            {
                var detail = new BookingDetail
                {
                    BookingId = booking.Id,
                    FieldId = (int)field.Id,
                    StartTime = (DateTime)field.Start,
                    EndTime = (DateTime)field.End
                };
                _context.BookingDetails.Add(detail);
            }
            await _context.SaveChangesAsync();
            //xóa viewmodel
            HttpContext.Session.Remove("CurrentBooking");

            return RedirectToAction("PaymentSuccess");
        }
        public IActionResult PaymentSuccess()
        {
            return View();
        }
        public IActionResult PaymentFail()
        {
            return View();
        }

        public static int CalculateMonthsDifference(DateTime beginTime, DateTime endTime)
        {
            int yearDifference = endTime.Year - beginTime.Year;
            int monthDifference = endTime.Month - beginTime.Month;

            int totalMonths = yearDifference * 12 + monthDifference;

            // Nếu ngày kết thúc nhỏ hơn ngày bắt đầu thì giảm đi một tháng
            if (endTime.Day < beginTime.Day)
            {
                totalMonths--;
            }

            return totalMonths;
        }
    }
}
