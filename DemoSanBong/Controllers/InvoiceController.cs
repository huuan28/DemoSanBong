using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace DemoSanBong.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private InvoiceViewModel currentInvoice;
        public InvoiceController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
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
                };
                model.SelectedFields.SelectedFields.Add(new FieldViewModel
                {
                    Id = field.Id,
                    Type = field.Type,
                    Name = field.Name,
                    Price = (booking.RentalType == 0) ? field.GetPrice(_context, booking.CreateDate) : field.GetPricePerMonth(_context, booking.CreateDate)
                });
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
                CheckinDate = booking.CheckinDate,
                CheckoutDate = booking.CheckoutDate,
                CashierId = cashier.Id,
                VAT = 10
            };
            booking.Status = 4;
            _context.Update(booking);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = invoice.Id });
        }
        public IActionResult Details(int id)
        {
            var inv = _context.Invoices.Find(id);
            if (inv == null)
                return NotFound();
            currentInvoice = GetInvoiceFromSession(id);
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
            return View(currentInvoice);
        }
        public IActionResult InvoiceList()
        {
            var list = _context.Invoices.ToList();
            return View(list);
        }
        public IActionResult Checkout(int id)
        {
            return View();
        }

        /////////Sesstion
        private InvoiceViewModel GetInvoiceFromSession(int id)
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
                }
                model.Services = _context.Services.ToList();

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
                }
                model.Services = _context.Services.ToList();
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
        public IActionResult AddService(int ivId, int svId, int qty)
        {
            int q = qty == 0 ? 1 : qty;
            currentInvoice = GetInvoiceFromSession(ivId);
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
            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }
        public IActionResult IncreaseService(int ivId, int svId)
        {
            currentInvoice = GetInvoiceFromSession(ivId);
            var invoiceSv = _context.InvoiceServices.FirstOrDefault(i => i.InvoiceId == ivId && i.ServiceId == svId);

            if (invoiceSv != null)
            {
                invoiceSv.Quantity += 1;
                invoiceSv.Service = _context.Services.Find(invoiceSv.ServiceId);
                _context.InvoiceServices.Update(invoiceSv);
                _context.SaveChanges();
                var ivsv = currentInvoice.Order.FirstOrDefault(i => i.ServiceId == svId);
                currentInvoice.Order.Remove(ivsv);
                currentInvoice.Order.Add(invoiceSv);
                currentInvoice.Order = currentInvoice.Order.OrderBy(i => i.ServiceId).ToList();
            }

            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }

        public IActionResult DecreaseService(int ivId, int svId)
        {
            currentInvoice = GetInvoiceFromSession(ivId);
            var invoiceSv = _context.InvoiceServices.FirstOrDefault(i => i.InvoiceId == ivId && i.ServiceId == svId);

            if (invoiceSv != null && invoiceSv.Quantity > 1)
            {
                invoiceSv.Quantity -= 1;
                invoiceSv.Service = _context.Services.Find(invoiceSv.ServiceId);
                _context.InvoiceServices.Update(invoiceSv);
                _context.SaveChanges();
                var ivsv = currentInvoice.Order.FirstOrDefault(i => i.ServiceId == svId);
                currentInvoice.Order.Remove(ivsv);
                currentInvoice.Order.Add(invoiceSv);
                currentInvoice.Order = currentInvoice.Order.OrderBy(i => i.ServiceId).ToList();
            }
            else if (invoiceSv != null && invoiceSv.Quantity == 1)
            {
                _context.InvoiceServices.Remove(invoiceSv);
                var ivsv = currentInvoice.Order.FirstOrDefault(i => i.ServiceId == svId);
                currentInvoice.Order.Remove(ivsv);
                _context.SaveChanges();
            }

            SaveInvoiceToSession(currentInvoice);
            return PartialView("_SelectedServices", currentInvoice.Order);
        }
    }
}
