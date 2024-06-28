using DemoSanBong.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace DemoSanBong
{
    public class SeedData
    {
        public static async Task Initialize(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IApplicationBuilder app)
        {
            #region Seeding User
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }
            if (!await roleManager.RoleExistsAsync("Cashier"))
            {
                await roleManager.CreateAsync(new IdentityRole("Cashier"));
            }
            // Tạo tài khoản admin mặc định nếu chưa tồn tại
            if (userManager.Users.All(u => u.UserName != "admin"))
            {
                var admin = new AppUser { UserName = "admin", Email = "admin@example.com", PhoneNumber = "0123456789", FullName = "Quản trị viên" };
                var result = await userManager.CreateAsync(admin, "admin123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Danh sách các khách hàng cần thêm
            var customers = new List<AppUser>
            {
                 new AppUser { UserName = "huuan",FullName = "An", Email = "nghuuan2803@gmail.com", PhoneNumber = "0909090909" },
                 new AppUser { UserName = "anhhao",FullName = "Hào", Email = "customer2@example.com", PhoneNumber = "0303030303" },
                 new AppUser { UserName = "hien",FullName = "Hiền", Email = "customer3@example.com", PhoneNumber = "0123012345" },
             };
            var password = "123123";

            foreach (var customer in customers)
            {
                if (userManager.Users.All(u => u.UserName != customer.UserName))
                {
                    var result = await userManager.CreateAsync(customer, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(customer, "Customer");
                    }
                }
            }
            if (userManager.Users.All(u => u.UserName != "thungan"))
            {
                var user = new AppUser { UserName = "thungan", FullName = "Thu Ngân" };
                var result = await userManager.CreateAsync(user, "123123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Cashier");
                }
            }
            #endregion

            AppDbContext context = app.ApplicationServices.
               CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            #region Seeding Field
            if (!context.Fields.Any())
            {
                context.Fields.AddRange(
                   new Field { Name = "Sân 1", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 2", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 3", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 4", Type = "trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 5", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 6", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 7", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 8", Type = "trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 9", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 10", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 11", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "Sân 12", Type = "trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" }
                    );
                await context.SaveChangesAsync();

                if (!context.FieldRates.Any())
                {
                    foreach (var field in context.Fields)
                    {
                        context.FieldRates.AddRange(
                            new FieldRate { EffectiveDate = DateTime.Now, Field = field, FieldId = field.Id, Type = 0, Price = 100000 },
                            new FieldRate { EffectiveDate = DateTime.Now, Field = field, FieldId = field.Id, Type = 1, Price = 2000000 }
                            );
                    }
                    await context.SaveChangesAsync();
                }
                if (!context.FieldImages.Any())
                {
                    foreach (var field in context.Fields)
                    {
                        context.FieldImages.AddRange(
                        new FieldImage
                        {
                            FieldId = field.Id,
                            Field = field,
                            FileName = "default.jpg",
                            IsDefault = true,
                        },
                        new FieldImage
                        {
                            FieldId = field.Id,
                            Field = field,
                            FileName = "2nd.jpg",
                            IsDefault = false
                        });
                    }
                }
                await context.SaveChangesAsync();
            }
            #endregion
            #region seeding service
            if (!context.Services.Any())
            {
                context.Services.AddRange
                    (
                    new Service
                    {
                        Name = "Sting",
                        Description = "Description for service 1",
                        Type = "DoAn",
                        Unit = "chai",
                        Quantity = 10,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image1.jpg"
                    },
                    new Service
                    {
                        Name = "Service 2",
                        Description = "Description for service 2",
                        Type = "Type B",
                        Unit = "lon",
                        Quantity = 20,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image2.jpg"
                    },
                    new Service
                    {
                        Name = "Service 3",
                        Description = "Description for service 3",
                        Type = "Type C",
                        Unit = "lon",
                        Quantity = 30,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image3.jpg"
                    },
                    new Service
                    {
                        Name = "Service 4",
                        Description = "Description for service 4",
                        Type = "Type D",
                        Unit = "lon",
                        Quantity = 40,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image4.jpg"
                    },
                    new Service
                    {
                        Name = "Service 5",
                        Description = "Description for service 5",
                        Type = "Type E",
                        Unit = "lon",
                        Quantity = 50,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image5.jpg"
                    },
                    new Service
                    {
                        Name = "Service 6",
                        Description = "Description for service 6",
                        Type = "Type F",
                        Unit = "lon",
                        Quantity = 60,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image6.jpg"
                    },
                    new Service
                    {
                        Name = "Service 7",
                        Description = "Description for service 7",
                        Type = "Type G",
                        Unit = "lon",
                        Quantity = 70,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image7.jpg"
                    },
                    new Service
                    {
                        Name = "Service 8",
                        Description = "Description for service 8",
                        Type = "Type H",
                        Unit = "lon",
                        Quantity = 80,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image8.jpg"
                    },
                    new Service
                    {
                        Name = "Service 9",
                        Description = "Description for service 9",
                        Type = "Type I",
                        Unit = "lon",
                        Quantity = 90,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image9.jpg"
                    },
                    new Service
                    {
                        Name = "Service 10",
                        Description = "Description for service 10",
                        Type = "Type J",
                        Unit = "lon",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        ImagePath = "path/to/image10.jpg"
                    }
                );
                await context.SaveChangesAsync();
                if (!context.ServiceRates.Any())
                {
                    foreach (var service in context.Services)
                    {
                        context.ServiceRates.Add(
                            new ServiceRate { EffectiveDate = DateTime.Now, Service = service, ServiceId = service.Id, Price = 15000 }
                            );
                    }
                    await context.SaveChangesAsync();
                }
            }
            #endregion

            //Set rules
            if (!context.Rules.Any())
            {
                context.Rules.Add(new Rules
                {
                    OpenTime = 7,
                    CloseTime = 22,
                    DepositPercent = 20
                });
                await context.SaveChangesAsync();
                Console.WriteLine("Set Rules Successed");
            }

            if (!context.Bookings.Any(i=> i.CreateDate.Date == DateTime.Today))
            {

                var bookings = new List<Booking>
            {
                new Booking
                {
                    CusID = context.Users.Where(i=> i.FullName=="An").FirstOrDefault().Id,
                    CheckinDate = DateTime.Today.AddHours(7),
                    CheckoutDate = DateTime.Today.AddHours(22),
                    CreateDate = DateTime.Now,
                    PaymentMethod = 1,
                    Status = 1,
                    RentalType = 0,
                    Deposit = 0
                },
                new Booking
                {
                    CusID = context.Users.Where(i=> i.FullName=="An").FirstOrDefault().Id,
                    CheckinDate = DateTime.Today.AddHours(9).AddDays(1),
                    CheckoutDate = DateTime.Today.AddHours(21).AddDays(1),
                    CreateDate = DateTime.Now.AddDays(1),
                    PaymentMethod = 1,
                    Status = 1,
                    RentalType = 0,
                    Deposit = 0
                },
            };
                context.Bookings.AddRange(bookings);
                await context.SaveChangesAsync();

                foreach (var booking in bookings)
                {
                    var detail1 = new BookingDetail
                    {
                        BookingId = booking.Id,
                        FieldId = 3
                    };
                    var detail2 = new BookingDetail
                    {
                        BookingId = booking.Id,
                        FieldId = 4
                    };
                    context.BookingDetails.Add(detail1);
                    context.BookingDetails.Add(detail2);
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

