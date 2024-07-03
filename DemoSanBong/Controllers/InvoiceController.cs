using DemoSanBong.Models;
using DemoSanBong.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DemoSanBong.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
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

        [Authorize(Roles ="Cashier")]
        public async Task<IActionResult> Checkin(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
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
            return RedirectToAction("Details", new {id = invoice.Id});
        }
        public IActionResult Details(int id)
        {
            var inv = _context.Invoices.Find(id);
            if (inv == null)
                return NotFound();
            var model = new InvoiceViewModel
            {
                Id = inv.Id,
                CreateDate = inv.CreateDate,
                CheckinDate = inv.CheckinDate,
                CheckoutDate = inv.CheckoutDate,
                Note = inv.Note,
                Status = inv.Status
            };
            model.Booking = _context.Bookings.Find(inv.BookingId);
            model.Booking.Customer = _context.Users.Find(inv.Booking.CusID);
            model.Cashier = _context.Users.FirstOrDefault(i => i.Id == inv.CashierId);
            var bkDetail = _context.BookingDetails.Where(i=>i.BookingId == inv.BookingId).ToList();
            model.Fields = new List<FieldViewModel>();
            foreach (var item in bkDetail)
            {
                var field = _context.Fields.FirstOrDefault(i => i.Id == item.FieldId);
                var modelfield = new FieldViewModel
                {
                    Id = field.Id,
                    Type = field.Type,
                    Name = field.Name,
                };
                model.Fields.Add(new FieldViewModel
                {
                    Id = field.Id,
                    Type = field.Type,
                    Name = field.Name,
                    Price = (model.Booking.RentalType == 0) ? field.GetPrice(_context, model.Booking.CreateDate) : field.GetPricePerMonth(_context, model.Booking.CreateDate)
                });
            }
            model.Services = _context.Services.ToList();
            model.Order = new List<Service>();
            return View(model);
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
    }
}
