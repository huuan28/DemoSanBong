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
                    await userManager.AddToRoleAsync(admin, "Cashier");
                }
            }

            // Danh sách các khách hàng cần thêm
            var customers = new List<AppUser>
            {
                 new AppUser { UserName = "huuan",FullName = "An", Email = "nghuuan2803@gmail.com", PhoneNumber = "0933912012" },
                 new AppUser { UserName = "anhhao",FullName = "Hào", Email = "customer2@example.com", PhoneNumber = "0303030303" },
                 new AppUser { UserName = "hien",FullName = "Hiền", Email = "customer3@example.com", PhoneNumber = "0123012345" },
                 new AppUser { UserName = "qmanh",FullName = "Mạnh", Email = "customer4@example.com", PhoneNumber = "0123012355" },
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
                   new Field { Name = "S501", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S502", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S503", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S504", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S505", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S701", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S702", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S703", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S704", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S705", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S901", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S902", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S903", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S904", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "S905", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "SK01", Type = "Trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "SK02", Type = "Trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "SK03", Type = "Trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "SK04", Type = "Trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" },
                   new Field { Name = "SK05", Type = "Trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/" }
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
                        Name = "Sting dâu",
                        Description = "Sting dâu",
                        Type = "Nước",
                        Unit = "chai",
                        Quantity = 10,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Sting vàng",
                        Description = "Description for service 2",
                        Type = "Nước",
                        Unit = "chai",
                        Quantity = 20,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "7UP chanh",
                        Description = "Description for service 3",
                        Type = "Nước",
                        Unit = "lon",
                        Quantity = 30,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Bò húc Thái",
                        Description = "Description for service 4",
                        Type = "Nước",
                        Unit = "lon",
                        Quantity = 40,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Number1 vàng",
                        Description = "Description for service 5",
                        Type = "Nước",
                        Unit = "chai",
                        Quantity = 50,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Sữa đậu nành Number1",
                        Description = "Description for service 6",
                        Type = "Nước",
                        Unit = "chai",
                        Quantity = 60,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Boncha mật ong",
                        Description = "Description for service 7",
                        Type = "Nước",
                        Unit = "chai",
                        Quantity = 70,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Boncha việt quất",
                        Description = "Description for service 8",
                        Type = "Nước",
                        Unit = "chai",
                        Quantity = 80,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Bánh mì ngọt",
                        Description = "Description for service 9",
                        Type = "Đồ ăn",
                        Unit = "bịch",
                        Quantity = 90,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Bánh mì heo quay",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "ổ",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                       
                    },
                    new Service
                    {
                        Name = "Bánh mì heo quay",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "ổ",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Bánh mì trứng ốp la",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "ổ",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Bánh mì chả cá",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "ổ",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Cơm chiên gà",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "đĩa",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                       
                    },
                    new Service
                    {
                        Name = "Cơm chiên dương châu",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "đĩa",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Cơm chiên bò xào",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "đĩa",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Cơm tấm sườn",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "đĩa",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Cơm tấm sườn - bì - chả - trứng",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "đĩa",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Phở bò",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "tô",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
                    },
                    new Service
                    {
                        Name = "Bún bò",
                        Description = "Description for service 10",
                        Type = "Đồ ăn",
                        Unit = "tô",
                        Quantity = 100,
                        CreateDate = DateTime.Now,
                        
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
            if (!context.Parameters.Any())
            {
                context.Parameters.Add(new Parameter
                {
                    OpenTime = 7,
                    CloseTime = 22,
                    DepositPercent = 20
                });
                await context.SaveChangesAsync();
                Console.WriteLine("Set Rules Successed");
            }

            //if (!context.Bookings.Any(i=> i.CreateDate.Date == DateTime.Today))
            //{

            //    var bookings = new List<Booking>
            //{
            //    new Booking
            //    {
            //        CusID = context.Users.Where(i=> i.FullName=="An").FirstOrDefault().Id,
            //        CheckinDate = DateTime.Today.AddHours(7),
            //        CheckoutDate = DateTime.Today.AddHours(22),
            //        CreateDate = DateTime.Now,
            //        PaymentGate = 1,
            //        Status = 1,
            //        RentalType = 0,
            //        Deposit = 0
            //    },
            //    new Booking
            //    {
            //        CusID = context.Users.Where(i=> i.FullName=="An").FirstOrDefault().Id,
            //        CheckinDate = DateTime.Today.AddHours(9).AddDays(1),
            //        CheckoutDate = DateTime.Today.AddHours(21).AddDays(1),
            //        CreateDate = DateTime.Now.AddDays(1),
            //        PaymentGate = 1,
            //        Status = 1,
            //        RentalType = 0,
            //        Deposit = 0
            //    },
            //};
            //    context.Bookings.AddRange(bookings);
            //    await context.SaveChangesAsync();

            //    foreach (var booking in bookings)
            //    {
            //        var detail1 = new BookingDetail
            //        {
            //            BookingId = booking.Id,
            //            FieldId = 3
            //        };
            //        var detail2 = new BookingDetail
            //        {
            //            BookingId = booking.Id,
            //            FieldId = 4
            //        };
            //        context.BookingDetails.Add(detail1);
            //        context.BookingDetails.Add(detail2);
            //    }
            //    await context.SaveChangesAsync();
            //}
        }
    }
}

