using DemoSanBong.Models;
using DemoSanBong.Services;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
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
                currentBooking.SelectedFields.Amount -= f.PricePerMonth * dif;
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
            //ViewBag.Message = message;
            SaveBookingToSession(currentBooking);
            return View(currentBooking);
        }

        [HttpPost]
        public async Task<IActionResult> Booking(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                currentBooking = GetBookingFromSession();
                currentBooking.FullName = model.FullName;
                currentBooking.PhoneNumber = model.PhoneNumber;
                currentBooking.RentalType = model.RentalType;
                SaveBookingToSession(currentBooking);
                if (model.CheckoutDate <= model.CheckinDate)
                {
                    currentBooking.SelectedFields.Amount = 0;
                    currentBooking.SelectedFields.Deposit = 0;
                    currentBooking.SelectedFields.SelectedFields.Clear();
                    SaveBookingToSession(currentBooking);
                    ViewBag.Message = "Giờ đặt sân không hợp lệ!";
                    return View(currentBooking);
                }
                if (currentBooking.SelectedFields.SelectedFields.Count == 0)
                {
                    ViewBag.Message = "Chưa chọn sân!";
                    return View(currentBooking);
                }
                var user = _context.Users.FirstOrDefault(i => i.PhoneNumber == model.PhoneNumber);
                //Kiểm tra đã đăng ký chưa
                if (user == null)
                {
                    if (!await CreateUnRegisterUser(model.PhoneNumber, model.FullName)) ; //lưu tài khoản loại chưa đăng ký
                    user = await _userManager.FindByNameAsync(model.PhoneNumber);
                }
                else if (user.FullName != model.FullName && user.IsRegisted)
                {
                    ViewBag.Message = "SĐT này đã sử dụng với tên khác, hãy đăng nhập để cập nhật họ và tên!";
                    return View(currentBooking);
                }
                var rules = _context.Parameters.FirstOrDefault();

                

                //Kiểm tra lại sân trống
                var removefield = new List<FieldViewModel>();
                foreach (var i in currentBooking.SelectedFields.SelectedFields)
                {
                    if (!IsAvailable((int)i.Id, i.Start.Value, i.End.Value))
                    {
                        removefield.Add(i);                        
                    }
                }
                if (removefield.Count > 0)
                {
                    foreach (var i in removefield)
                    {
                        currentBooking.SelectedFields.SelectedFields.Remove(i);
                        currentBooking.SelectedFields.Amount -= i.Price * (i.End.Value.Hour - i.Start.Value.Hour);
                        currentBooking.SelectedFields.Deposit -= 0.2*i.Price * (i.End.Value.Hour - i.Start.Value.Hour);
                    }
                    SaveBookingToSession(currentBooking);
                    ViewBag.Message = $"Đã xảy ra lỗi, vui lòng chọn sân khác";
                    return View(currentBooking);
                }

                SaveBookingToSession(currentBooking);
                var vnPayModel = new VnPaymentRequestModel();
                vnPayModel.Amount = (double)currentBooking.Deposit;
                vnPayModel.CreateDate = DateTime.Now;
                vnPayModel.Description = $"{model.PhoneNumber}-{model.FullName}";
                vnPayModel.FullName = model.FullName;
                var bookings = _context.Bookings.FirstOrDefault();
                if (bookings != null)

                    vnPayModel.BookingId = _context.Bookings.Max(i => i.Id) + 1;
                else
                    vnPayModel.BookingId = 1;
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

        //kết quả trả về từ VNPAY
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
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> History()
        {
            var user = await GetCurrentUserAsync();
            var list = _context.Bookings.Where(i => i.CusID == user.Id).Include(i => i.Customer).ToList();
            return View(list);
        }
        public IActionResult Details(int id, string? message)
        {
            var booking = _context.Bookings.FirstOrDefault(i => i.Id == id);
            if (booking == null) return NotFound();
            var model = new BookingViewModel
            {
                Id = (int)booking.Id,
                CheckinDate = booking.CheckinDate,
                CheckoutDate = booking.CheckoutDate,
                Deposit = booking.Deposit,
                Customer = _context.Users.FirstOrDefault(i => i.Id == booking.CusID),
                RentalType = booking.RentalType,
                Status = booking.Status
            };
            var fields = _context.BookingDetails.Where(i => i.BookingId == id).ToList();
            model.SelectedFields = new SelectedFieldsViewModel
            {
                SelectedFields = new List<FieldViewModel>()
            };
            foreach (var item in fields)
            {
                var field = _context.Fields.FirstOrDefault(i => i.Id == item.FieldId);
                var modelfield = new FieldViewModel
                {
                    Id = field.Id,
                    Type = field.Type,
                    Name = field.Name,
                    Start = item.StartTime,
                    End = item.EndTime,
                    Price = (booking.RentalType == 0) ? field.GetPrice(_context, booking.CreateDate) : field.GetPricePerMonth(_context, booking.CreateDate)
                };
                model.SelectedFields.SelectedFields.Add(modelfield);
            }
            ViewBag.Message = message;
            return View(model);
        }
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await GetCurrentUserAsync();
            var bk = _context.Bookings.Find(id);
            if (bk != null && bk.CusID == user.Id && (bk.Status == 1 || bk.Status == 2 ))
            {
                bk.Status = 3;
                _context.Bookings.Update(bk);
                _context.SaveChanges();
                return RedirectToAction("Details", new { id = id, message = "Hủy thành công!" });
            }
            return RedirectToAction("Details", new { id = id, message = "Không thể hủy!" });
        }
    }
}
