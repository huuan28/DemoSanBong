using DemoSanBong.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DemoSanBong
{
    public class SeedData
    {
        public static async Task Initialize(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IApplicationBuilder app)
        {
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
                 new AppUser { UserName = "an",FullName = "An", Email = "nghuuan2803@gmail.com" },
                 new AppUser { UserName = "hao",FullName = "Hào", Email = "customer2@example.com" },
                 new AppUser { UserName = "hien",FullName = "Hiền", Email = "customer3@example.com" },
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

            AppDbContext context = app.ApplicationServices.
               CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            if (!context.Fields.Any())
            {
                context.Fields.AddRange(
                   new Field { Name = "Sân 1", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/1/" },
                   new Field { Name = "Sân 2", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/2/" },
                   new Field { Name = "Sân 3", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/3/" },
                   new Field { Name = "Sân 4", Type = "trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/4/" },
                   new Field { Name = "Sân 5", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/5/" },
                   new Field { Name = "Sân 6", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/6/" },
                   new Field { Name = "Sân 7", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/7/" },
                   new Field { Name = "Sân 8", Type = "trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/8/" },
                   new Field { Name = "Sân 9", Type = "5 người", Description = "Sân cho 5 người", IsActive = true, ImagePath = "Images/9/" },
                   new Field { Name = "Sân 10", Type = "7 người", Description = "Sân cho 7 người", IsActive = true, ImagePath = "Images/10/" },
                   new Field { Name = "Sân 11", Type = "9 người", Description = "Sân cho 9 người", IsActive = true, ImagePath = "Images/11/" },
                   new Field { Name = "Sân 12", Type = "trẻ em", Description = "Sân cho trẻ em", IsActive = true, ImagePath = "Images/12/" }
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
        }
    }
}

