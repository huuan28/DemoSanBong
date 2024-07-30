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
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private InvoiceViewModel currentInvoice;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        public InvoiceController(IMomoService momoService,UserManager<AppUser> userManager, AppDbContext context, IVnPayService vnPayService)
        {
            _userManager = userManager;
            _context = context;
            _vnPayService = vnPayService;
            _momoService = momoService;
        }
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
                    Status = booking.Status
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
                    Start=item.StartTime,
                    End=item.EndTime,
                    Price = (booking.RentalType == 0) ? field.GetPrice(_context, booking.CreateDate) : field.GetPricePerMonth(_context, booking.CreateDate)
                };
                model.SelectedFields.SelectedFields.Add(modelfield);
            }
            return View(model);
        }

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
            currentInvoice.Booking = _context.Bookings.Find(inv.BookingId);
            currentInvoice.Booking.Customer = _context.Users.Find(inv.Booking.CusID);
            currentInvoice.Cashier = _context.Users.FirstOrDefault(i => i.Id == inv.CashierId);
            var bkDetail = _context.BookingDetails.Where(i => i.BookingId == inv.BookingId).ToList();
            currentInvoice.Fields = new List<FieldViewModel>();
            foreach (var item in bkDetail)
            {
                var field = _context.Fields.FirstOrDefault(i => i.Id == item.FieldId);
                var modelfield = new FieldViewModel
                {
                    Id = field.Id,
                    Type = field.Type,
                    Name = field.Name,
                };
                currentInvoice.Fields.Add(new FieldViewModel
                {
                    Id = field.Id,
                    Type = field.Type,
                    Name = field.Name,
                    Price = (currentInvoice.Booking.RentalType == 0) ? field.GetPrice(_context, currentInvoice.Booking.CreateDate) : field.GetPricePerMonth(_context, currentInvoice.Booking.CreateDate)
                });
            }
            SaveInvoiceToSession(currentInvoice);
            ViewBag.Message = message;
            return View(currentInvoice);
        }
        public IActionResult InvoiceList(int? status)
        {
            var list = _context.Invoices.ToList();
            if (status!=null)
            {
                list = list.FindAll(i => i.Status == status);
            }
            return View(list);
        }

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
                model.Order = _context.InvoiceServices.Where(i => i.InvoiceId == id).ToList();
                foreach (var sv in model.Order)
                {
                    sv.Service = _context.Services.Find(sv.ServiceId);
                    sv.Service.SetCurrPrice(_context);
                }
                foreach (var ivsv in model.Order)
                {
                    ivsv.Service.SetCurrPrice(_context);
                }
                model.Services = _context.Services.ToList();
                foreach (var sv in model.Services)
                {
                    sv.SetCurrPrice(_context);
                }

                model.Booking = _context.Bookings.Find(inv.BookingId);
                model.Booking.Customer = _context.Users.Find(model.Booking.CusID);
                model.Cashier = await _userManager.GetUserAsync(HttpContext.User);


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
                model.Order = _context.InvoiceServices.Where(i => i.InvoiceId == id).ToList();
                foreach (var sv in model.Order)
                {
                    sv.Service = _context.Services.Find(sv.ServiceId);
                    sv.Service.SetCurrPrice(_context);
                }
                foreach (var ivsv in model.Order)
                {
                    ivsv.Service.SetCurrPrice(_context);
                }
                model.Services = _context.Services.ToList();
                foreach (var sv in model.Services)
                {
                    sv.SetCurrPrice(_context);
                }
                model.Booking = _context.Bookings.Find(inv.BookingId);
                model.Booking.Customer = _context.Users.Find(model.Booking.CusID);
                model.Cashier = await _userManager.GetUserAsync(HttpContext.User);

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
            var invoiceSv = _context.InvoiceServices.FirstOrDefault(i => i.InvoiceId == ivId && i.ServiceId == svId);
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
                invoiceSv.Service.SetCurrPrice(_context);
                _context.InvoiceServices.Add(invoiceSv);
                _context.SaveChanges();
                currentInvoice.Order.Add(invoiceSv);
            }
            else
            {
                invoiceSv.Quantity += q;
                _context.InvoiceServices.Update(invoiceSv);
                _context.SaveChanges();
                var ivsv = currentInvoice.Order.FirstOrDefault(i => i.ServiceId == svId);
                currentInvoice.Order.Remove(ivsv);
                currentInvoice.Order.Add(invoiceSv);
            }
            foreach (var ivsv in currentInvoice.Order)
            {
                ivsv.Service.SetCurrPrice(_context);
            }
            currentInvoice.Order=  currentInvoice.Order.OrderBy(i=>i.ServiceId).ToList();
            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }
        public async Task<IActionResult> IncreaseService(int ivId, int svId)
        {
            currentInvoice = await GetInvoiceFromSession(ivId);
            var invoiceSv = _context.InvoiceServices.FirstOrDefault(i => i.InvoiceId == ivId && i.ServiceId == svId);

            if (invoiceSv != null)
            {
                invoiceSv.Quantity += 1;
                _context.InvoiceServices.Update(invoiceSv);
                _context.SaveChanges();

                var hdht = currentInvoice.Order.FirstOrDefault(i => i.InvoiceId == ivId && i.ServiceId == svId);
                hdht.Quantity += 1;
                hdht.Service.SetCurrPrice(_context);
            }
            foreach (var ivsv in currentInvoice.Order)
            {
                ivsv.Service.SetCurrPrice(_context);
            }
            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }
        public async Task<IActionResult> DecreaseService(int ivId, int svId)
        {
            currentInvoice = await GetInvoiceFromSession(ivId);
            var invoiceSv = _context.InvoiceServices.FirstOrDefault(i => i.InvoiceId == ivId && i.ServiceId == svId);

            if (invoiceSv != null && invoiceSv.Quantity > 1)
            {
                invoiceSv.Quantity -= 1;
                _context.InvoiceServices.Update(invoiceSv);
                _context.SaveChanges();
                var hdht = currentInvoice.Order.FirstOrDefault(i => i.InvoiceId == ivId && i.ServiceId == svId);
                hdht.Quantity -= 1;
                hdht.Service.SetCurrPrice(_context);
            }
            else if (invoiceSv != null && invoiceSv.Quantity == 1)
            {
                _context.InvoiceServices.Remove(invoiceSv);
                var ivsv = currentInvoice.Order.FirstOrDefault(i => i.ServiceId == svId);
                currentInvoice.Order.Remove(ivsv);
                _context.SaveChanges();
            }
            foreach (var ivsv in currentInvoice.Order)
            {
                ivsv.Service.SetCurrPrice(_context);
            }
            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {

            var iv = _context.Invoices.Find(id);
            iv.Status = 1;
            _context.Invoices.Update(iv);
            _context.SaveChanges();
            currentInvoice = await GetInvoiceFromSession(id);
            currentInvoice.Status = 1;
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
                    Amount = 1000000,
                    CreateDate = DateTime.Now,
                    Description = $"Thanh toan hoa don:{id}",
                    FullName = currentInvoice.Booking.Customer.FullName,
                    BookingId = id
                };
                redirectUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel,"");
            }
            else if (pm == 2)
            {
                var order = new OrderInfoModel
                {
                    FullName = currentInvoice.Booking.Customer.FullName,
                    Amount = 1000000, //giá để tạm chưa tính
                    OrderId = id.ToString(),
                    OrderInfo = $"Thanh toan hoa don:{id}",
                };
                var response = await _momoService.CreatePaymentAsync(order, "invoice");
                redirectUrl = response.PayUrl;
            }
            //tiền mặt để tạm chưa fix
            else
            {
                iv.Status = 2;
                _context.Invoices.Update(iv);
                _context.SaveChanges();
                HttpContext.Session.Remove("currentInvoice");
                redirectUrl = Url.Action("details",new {id=id,message="Thanh toán thành công"});
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

            foreach (var item in currentInvoice.Fields)
            {
                iv.Amount += item.Price;
            }
            foreach (var i in currentInvoice.Order)
            {
                i.Service.SetCurrPrice(_context);
                iv.Amount += i.Service.GetCurrPrice();
            }
            _context.Invoices.Update(iv);
            _context.SaveChanges();
            //xóa viewmodel
            HttpContext.Session.Remove("currentInvoice");

            return RedirectToAction("Details", new { id = id, message = "Thanh toán thành công" });
        }
        public async Task<IActionResult> MomoPaymentCallBack()
        {
            // Lấy thông tin từ query string để xác thực và cập nhật trạng thái đơn hàng
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            if (response == null||response.ErrorCode!=0)
            {
                //thanh toán thất bại
                int i = (int)TempData["InvoiceId"];
                return RedirectToAction("details", new {id = i, message = "Thanh toán thất bại"});
            }
            string[] rs = response.OrderInfo.Split(':');
            int id = int.Parse(rs[rs.Length-1]);
            //lấy thông tin đặt phòng từ viewmodel
            currentInvoice = await GetInvoiceFromSession(id);

            var iv = _context.Invoices.FirstOrDefault(x => x.Id == currentInvoice.Id);
            currentInvoice.Status = 2;
            SaveInvoiceToSession(currentInvoice);
            iv.Status = 2;
            iv.CashierId = currentInvoice.Cashier.Id;

            foreach (var item in currentInvoice.Fields)
            {
                iv.Amount += item.Price;
            }
            foreach (var i in currentInvoice.Order)
            {
                i.Service.SetCurrPrice(_context);
                iv.Amount += i.Service.GetCurrPrice()*i.Quantity;
            }
            _context.Invoices.Update(iv);
            _context.SaveChanges();
            //xóa session viewmodel
            HttpContext.Session.Remove("currentInvoice");

            return RedirectToAction("Details", new { id = id, message = "Thanh toán thành công" });
        }
    }
}
