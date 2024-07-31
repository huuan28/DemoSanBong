using DemoSanBong.Models;
using DemoSanBong.Services;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace DemoSanBong.Controllers
{
    public class InvoiceController : Controller
    {
        #region field and Constructor
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private InvoiceViewModel currentInvoice;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        public InvoiceController(IMomoService momoService, UserManager<AppUser> userManager, AppDbContext context, IVnPayService vnPayService)
        {
            _userManager = userManager;
            _context = context;
            _vnPayService = vnPayService;
            _momoService = momoService;
        }
        #endregion

        #region Xem đặt sân
        // danh sách đặt sân
        public IActionResult Index()
        {
            var list = _context.Bookings.ToList();
            var models = new List<BookingViewModel>();
            foreach (var booking in list)
            {
                models.Add(new BookingViewModel
                {
                    Id = (int)booking.Id,
                    CheckinDate = booking.CheckinDate,
                    CheckoutDate = booking.CheckoutDate,
                    Deposit = booking.Deposit,
                    Customer = _context.Users.FirstOrDefault(i => i.Id == booking.CusID),
                    RentalType = booking.RentalType,
                    Status = booking.Status,
                    CreateDate = booking.CreateDate
                });
            }
            return View(models);
        }
        //kiểm tra đặt sân
        public IActionResult Check(int id)
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
            return View(model);
        }
        #endregion

        #region Phiếu nhận sân
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> Checkin(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null || booking.Status == 3 || booking.Status == 4)
                return NotFound();
            var cashier = await _userManager.GetUserAsync(HttpContext.User);
            var invoice = new Invoice
            {
                BookingId = booking.Id,
                CreateDate = DateTime.Now,
                CheckinDate = DateTime.Now,
                CheckoutDate = null,
                CashierId = cashier.Id,
                VAT = 10
            };
            booking.Status = 4;
            booking.CheckinDate = DateTime.Now;
            _context.Update(booking);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = invoice.Id });
        }
        public async Task<IActionResult> Details(int id, string? message)
        {
            var inv = _context.Invoices.Find(id);
            if (inv == null)
                return NotFound();
            currentInvoice = await GetInvoiceFromSession(id);
            ViewBag.Message = message;
            return View(currentInvoice);
        }
        public IActionResult InvoiceList(int? status)
        {
            var list = _context.Invoices.Include(i=>i.Booking).ThenInclude(i=>i.Customer).Include(i=>i.Cashier).ToList();
            if (status != null)
            {
                list = list.FindAll(i => i.Status == status);
            }
            return View(list);
        }
        #endregion

        #region Session phiếu/hóa đơn
             /////////Sesstion
        private async Task<InvoiceViewModel> GetInvoiceFromSession(int id)
        {
            var InvoiceJson = HttpContext.Session.GetString("CurrentInvoice");
            InvoiceViewModel model;
            var inv = _context.Invoices.Find(id);
            if (string.IsNullOrEmpty(InvoiceJson))
            {
                model = new InvoiceViewModel
                {
                    Id = id,
                    CreateDate = inv.CreateDate,
                    CheckinDate = inv.CheckinDate,
                    CheckoutDate = inv.CheckoutDate,
                    Note = inv.Note,
                    Status = inv.Status
                };
                var ivsv = _context.InvoiceServices.Where(i => i.InvoiceId == id).Include(i => i.Service).ToList();
                model.Order = new List<InvoiceServiceView>();
                foreach (var i in ivsv)
                {
                    model.Order.Add(new InvoiceServiceView
                    {
                        InvoiceService = i,
                        Price = i.Service.getPrice(_context, DateTime.Now)
                    });
                }

                model.Booking = _context.Bookings.Find(inv.BookingId);
                model.Booking.Customer = _context.Users.Find(inv.Booking.CusID);
                model.Cashier = _context.Users.FirstOrDefault(i => i.Id == inv.CashierId);
                var bkDetail = _context.BookingDetails.Where(i => i.BookingId == inv.BookingId).ToList();
                model.Fields = new List<FieldViewModel>();
                foreach (var item in bkDetail)
                {
                    var field = _context.Fields.FirstOrDefault(i => i.Id == item.FieldId);
                    var modelfield = new FieldViewModel
                    {
                        Id = field.Id,
                        Type = field.Type,
                        Name = field.Name,
                        Start = item.StartTime,
                        End = item.EndTime,
                        Price = (model.Booking.RentalType == 0) ? field.GetPrice(_context, model.Booking.CreateDate) : field.GetPricePerMonth(_context, model.Booking.CreateDate)
                    };
                    if (model.Booking.RentalType == 0)
                    {
                        int stay = modelfield.End.Value.Hour - modelfield.Start.Value.Hour;
                        modelfield.Cost = stay * modelfield.Price;
                    }
                    else
                    {
                        int stay = CalculateMonthsDifference((DateTime)modelfield.Start, (DateTime)modelfield.End);
                        modelfield.Cost = stay * modelfield.Price;
                    }
                    model.Fields.Add(modelfield);
                }


                ///
                var svs = _context.Services.ToList();
                model.Services = new List<ServiceView>();
                foreach (var sv in svs)
                {
                    var svv = new ServiceView
                    {
                        Service = sv,
                        Price = sv.getPrice(_context, DateTime.Now)
                    };
                    model.Services.Add(svv);
                }

                model.Booking = _context.Bookings.Find(inv.BookingId);
                model.Booking.Customer = _context.Users.Find(model.Booking.CusID);
                model.Cashier = await _userManager.GetUserAsync(HttpContext.User);

                ////
                inv.Amount = 0;
                foreach (var i in model.Fields)
                {
                    if (model.Booking.RentalType == 0)
                    {
                        int stay = i.End.Value.Hour - i.Start.Value.Hour;
                        inv.Amount += i.Price * stay;
                    }
                    else
                    {
                        int stay = CalculateMonthsDifference(i.Start.Value, i.End.Value);
                        inv.Amount += i.Price * stay;
                    }
                }
                foreach (var i in model.Order)
                {
                    inv.Amount += i.Price * i.InvoiceService.Quantity;
                }

                model.Amount = inv.Amount;

                model.Final = inv.Amount - model.Booking.Deposit;

                _context.Invoices.Update(inv);
                _context.SaveChanges();
                return model;
            }
            model = JsonConvert.DeserializeObject<InvoiceViewModel>(InvoiceJson);
            if (model.Id != id)
            {
                model = new InvoiceViewModel
                {
                    Id = id,
                    CreateDate = inv.CreateDate,
                    CheckinDate = inv.CheckinDate,
                    CheckoutDate = inv.CheckoutDate,
                    Note = inv.Note,
                    Status = inv.Status
                };

                model.Booking = _context.Bookings.Find(inv.BookingId);
                model.Booking.Customer = _context.Users.Find(inv.Booking.CusID);
                model.Cashier = _context.Users.FirstOrDefault(i => i.Id == inv.CashierId);
                var bkDetail = _context.BookingDetails.Where(i => i.BookingId == inv.BookingId).ToList();
                model.Fields = new List<FieldViewModel>();
                foreach (var item in bkDetail)
                {
                    var field = _context.Fields.FirstOrDefault(i => i.Id == item.FieldId);
                    var modelfield = new FieldViewModel
                    {
                        Id = field.Id,
                        Type = field.Type,
                        Name = field.Name,
                        Start = item.StartTime,
                        End = item.EndTime,
                        Price = (model.Booking.RentalType == 0) ? field.GetPrice(_context, model.Booking.CreateDate) : field.GetPricePerMonth(_context, model.Booking.CreateDate)
                    };
                    if (model.Booking.RentalType == 0)
                    {
                        int stay = modelfield.End.Value.Hour - modelfield.Start.Value.Hour;
                        modelfield.Cost = stay * modelfield.Price;
                    }
                    else
                    {
                        int stay = CalculateMonthsDifference((DateTime)modelfield.Start, (DateTime)modelfield.End);
                        modelfield.Cost = stay * modelfield.Price;
                    }
                    model.Fields.Add(modelfield);
                }


                ////
                var ivsv = _context.InvoiceServices.Where(i => i.InvoiceId == id).Include(i => i.Service).ToList();
                model.Order = new List<InvoiceServiceView>();
                foreach (var i in ivsv)
                {
                    model.Order.Add(new InvoiceServiceView
                    {
                        InvoiceService = i,
                        Price = i.Service.getPrice(_context, DateTime.Now)
                    });
                }
                var svs = _context.Services.ToList();
                model.Services = new List<ServiceView>();
                foreach (var sv in svs)
                {
                    var svv = new ServiceView
                    {
                        Service = sv,
                        Price = sv.getPrice(_context, DateTime.Now)
                    };
                    model.Services.Add(svv);
                }
                model.Booking = _context.Bookings.Find(inv.BookingId);
                model.Booking.Customer = _context.Users.Find(model.Booking.CusID);
                model.Cashier = await _userManager.GetUserAsync(HttpContext.User);

                ///
                inv.Amount = 0;
                foreach (var i in model.Fields)
                {
                    if (model.Booking.RentalType == 0)
                    {
                        int stay = i.End.Value.Hour - i.Start.Value.Hour;
                        inv.Amount += i.Price * stay;
                    }
                    else
                    {
                        int stay = CalculateMonthsDifference(i.Start.Value, i.End.Value);
                        inv.Amount += i.Price * stay;
                    }
                }
                foreach (var i in model.Order)
                {
                    inv.Amount += i.Price * i.InvoiceService.Quantity;
                }

                model.Amount = inv.Amount;

                model.Final = inv.Amount - model.Booking.Deposit;

                _context.Invoices.Update(inv);
                _context.SaveChanges();
            }
            return model;
        }

        //Lưu thông tin đặt sân vào session
        private void SaveInvoiceToSession(InvoiceViewModel invoice)
        {
            var InvoiceJson = JsonConvert.SerializeObject(invoice);
            HttpContext.Session.SetString("CurrentInvoice", InvoiceJson);
        }
             //Session////////
        [HttpPost]
        public async Task<IActionResult> AddService(int ivId, int svId, int qty)
        {
            int q = qty == 0 ? 1 : qty;
            currentInvoice = await GetInvoiceFromSession(ivId);
            var sv = _context.Services.Find(svId);
            var invoiceSv = _context.InvoiceServices.Where(i => i.InvoiceId == ivId && i.ServiceId == svId).Include(i=>i.Service).FirstOrDefault();
            if (invoiceSv.Service.Quantity >= q)
            {
                if (invoiceSv == null)
                {
                    invoiceSv = new InvoiceService
                    {
                        InvoiceId = ivId,
                        Invoice = _context.Invoices.Find(ivId),
                        Service = sv,
                        ServiceId = svId,
                        Quantity = q,
                        OrderDate = DateTime.Now,
                    };
                    invoiceSv.Service.Quantity -= q;
                    _context.Services.Update(sv);
                    _context.InvoiceServices.Add(invoiceSv);
                    _context.SaveChanges();
                    currentInvoice.Order.Add(new InvoiceServiceView
                    {
                        InvoiceService = invoiceSv,
                        Price = sv.getPrice(_context, DateTime.Now)
                    });
                }
                else
                {
                    invoiceSv.Quantity += q;
                    invoiceSv.Service.Quantity -= q;
                    _context.Services.Update(sv);
                    _context.InvoiceServices.Update(invoiceSv);
                    _context.SaveChanges();
                    var ivsv = currentInvoice.Order.FirstOrDefault(i => i.InvoiceService.ServiceId == svId);
                    ivsv.InvoiceService.Quantity += q;
                }
            }
            currentInvoice.Order = currentInvoice.Order.OrderBy(i => i.InvoiceService.ServiceId).ToList();
            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }
        #endregion

        #region Thao tác dịch vụ
        public async Task<IActionResult> IncreaseService(int ivId, int svId)
        {
            currentInvoice = await GetInvoiceFromSession(ivId);
            var invoiceSv = _context.InvoiceServices.Where(i => i.InvoiceId == ivId && i.ServiceId == svId).Include(i=>i.Service).FirstOrDefault();

            if (invoiceSv != null&&invoiceSv.Service.Quantity>=1)
            {
                invoiceSv.Quantity += 1;
                invoiceSv.Service.Quantity -= 1;
                _context.InvoiceServices.Update(invoiceSv);
                _context.Services.Update(invoiceSv.Service);
                _context.SaveChanges();

                var hdht = currentInvoice.Order.FirstOrDefault(i => i.InvoiceService.InvoiceId == ivId && i.InvoiceService.ServiceId == svId);
                hdht.InvoiceService.Quantity += 1;
            }

            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }
        public async Task<IActionResult> DecreaseService(int ivId, int svId)
        {
            currentInvoice = await GetInvoiceFromSession(ivId);
            var invoiceSv = _context.InvoiceServices.Where(i => i.InvoiceId == ivId && i.ServiceId == svId).Include(i=>i.Service).FirstOrDefault();

            if (invoiceSv != null && invoiceSv.Quantity > 1)
            {
                invoiceSv.Quantity -= 1;
                invoiceSv.Service.Quantity += 1;
                _context.InvoiceServices.Update(invoiceSv);
                _context.Services.Update(invoiceSv.Service);
                _context.SaveChanges();
                var hdht = currentInvoice.Order.FirstOrDefault(i => i.InvoiceService.InvoiceId == ivId && i.InvoiceService.ServiceId == svId);
                hdht.InvoiceService.Quantity -= 1;
            }
            else if (invoiceSv != null && invoiceSv.Quantity == 1)
            {
                invoiceSv.Service.Quantity += 1;
                _context.Services.Update(invoiceSv.Service);
                _context.InvoiceServices.Remove(invoiceSv);
                var ivsv = currentInvoice.Order.FirstOrDefault(i => i.InvoiceService.ServiceId == svId);
                currentInvoice.Order.Remove(ivsv);
                _context.SaveChanges();
            }
            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }
        #endregion

        #region Hóa đơn + thanh toán
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {

            var iv = _context.Invoices.Find(id);
            iv.Status = 1;
            currentInvoice = await GetInvoiceFromSession(id);
            iv.Amount = 0;
            foreach (var i in currentInvoice.Fields)
            {
                if (currentInvoice.Booking.RentalType == 0)
                {
                    int stay = i.End.Value.Hour - i.Start.Value.Hour;
                    iv.Amount += i.Price * stay;
                }
                else
                {
                    int stay = CalculateMonthsDifference(i.Start.Value, i.End.Value);
                    iv.Amount += i.Price * stay;
                }
            }
            foreach (var i in currentInvoice.Order)
            {
                iv.Amount += i.Price * i.InvoiceService.Quantity;
            }

            currentInvoice.Amount = iv.Amount;

            currentInvoice.Final = iv.Amount - currentInvoice.Booking.Deposit;
            currentInvoice.Status = 1;

            var booking = _context.Bookings.Find(currentInvoice.Booking.Id);
            booking.CheckoutDate = DateTime.Now;
            _context.Bookings.Update(booking);
            _context.Invoices.Update(iv);
            _context.SaveChanges();
            SaveInvoiceToSession(currentInvoice);
            return View(currentInvoice);
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(int id, int pm, string note)
        {
            currentInvoice = await GetInvoiceFromSession(id);
            currentInvoice.Note = note;
            var iv = _context.Invoices.Find(id);
            SaveInvoiceToSession(currentInvoice);
            string redirectUrl;
            if (pm == 1)
            {
                var vnPayModel = new VnPaymentRequestModel()
                {
                    Amount = (double)currentInvoice.Final,
                    CreateDate = DateTime.Now,
                    Description = $"Thanh toan hoa don:{id}",
                    FullName = currentInvoice.Booking.Customer.FullName,
                    BookingId = id
                };
                redirectUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel, "");
            }
            else if (pm == 2)
            {
                var order = new OrderInfoModel
                {
                    FullName = currentInvoice.Booking.Customer.FullName,
                    Amount = (double)currentInvoice.Final,
                    OrderId = id.ToString(),
                    OrderInfo = $"Thanh toan hoa don:{id}",
                };
                var response = await _momoService.CreatePaymentAsync(order, "invoice");
                redirectUrl = response.PayUrl;
            }
            else
            {
                iv.Status = 2;
                _context.Invoices.Update(iv);
                _context.SaveChanges();
                HttpContext.Session.Remove("CurrentInvoice");
                redirectUrl = Url.Action("details", new { id = id, message = "Thanh toán thành công" });
            }
            TempData["InvoiceId"] = id;
            return Json(new { redirectUrl });
        }
        public async Task<IActionResult> PaymentCallBack()
        {
            // Lấy thông tin từ query string của VnPay để xác thực và cập nhật trạng thái đơn hàng
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                int i = (int)TempData["InvoiceId"];
                return RedirectToAction("details", new { id = i, message = "Thanh toán thất bại" });
            }
            int id = int.Parse(response.OrderDescription.Split(':')[1]);
            //lấy thông tin đặt phòng từ viewmodel
            currentInvoice = await GetInvoiceFromSession(id);

            var iv = _context.Invoices.FirstOrDefault(x => x.Id == currentInvoice.Id);
            iv.Status = 2;
            iv.CashierId = currentInvoice.Cashier.Id;
            iv.Amount = (double)currentInvoice.Amount;
            _context.Invoices.Update(iv);
            _context.SaveChanges();
            //xóa session
            HttpContext.Session.Remove("CurrentInvoice");

            return RedirectToAction("Details", new { id = id, message = "Thanh toán thành công" });
        }
        public async Task<IActionResult> MomoPaymentCallBack()
        {
            // Lấy thông tin từ query string để xác thực và cập nhật trạng thái đơn hàng
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            if (response == null || response.ErrorCode != 0)
            {
                //thanh toán thất bại
                int i = (int)TempData["InvoiceId"];
                return RedirectToAction("details", new { id = i, message = "Thanh toán thất bại" });
            }
            string[] rs = response.OrderInfo.Split(':');
            int id = int.Parse(rs[rs.Length - 1]);
            //lấy thông tin đặt phòng từ viewmodel
            currentInvoice = await GetInvoiceFromSession(id);

            var iv = _context.Invoices.FirstOrDefault(x => x.Id == currentInvoice.Id);
            currentInvoice.Status = 2;
            SaveInvoiceToSession(currentInvoice);
            iv.Status = 2;
            iv.CashierId = currentInvoice.Cashier.Id;
            iv.Amount = (double)currentInvoice.Amount;
            _context.Invoices.Update(iv);
            _context.SaveChanges();
            //xóa session session
            HttpContext.Session.Remove("CurrentInvoice");

            return RedirectToAction("Details", new { id = id, message = "Thanh toán thành công" });
        }
        #endregion

        public async Task<IActionResult> PrintCheckin(int id)
        {
            currentInvoice =await GetInvoiceFromSession(id);
            return View(currentInvoice);
        }
        public async Task<IActionResult> PrintCheckout(int id)
        {
            currentInvoice = await GetInvoiceFromSession(id);
            return View(currentInvoice);
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
