using DemoSanBong.Models;
using DemoSanBong.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DemoSanBong
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(720);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
            builder.Services.AddScoped<IMomoService, MomoService>();
            var connectionString = builder.Configuration.GetConnectionString("SQLServerIdentityConnection") ?? throw new InvalidOperationException("Connection string 'SQLServerIdentityConnection' not found.");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
            //tài khoản người dùng
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;// Yêu cầu tài khoản được xác nhận là false
                options.Password.RequireDigit = false; //Yêu cầu chứa số là false
                options.Password.RequiredLength = 6;// Yêu cầu độ dài mật khẩu là 6 ký tự
                options.Password.RequireNonAlphanumeric = false;// Yêu cầu ký tự đặc biệt là false
                options.Password.RequireUppercase = false;// Yêu cầu chữ hoa là false
                options.Password.RequireLowercase = false;// Yêu cầu chữ thường là false
                options.User.RequireUniqueEmail = false;// Yêu cầu email là duy nhất
            }).AddEntityFrameworkStores<AppDbContext>();
            //Đăng ký dịch vụ VnPay
            builder.Services.AddSingleton<IVnPayService, VnPayService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            //Configuring Authentication Middleware to the Request Pipeline
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<AppUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    SeedData.Initialize(userManager, roleManager, app).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
            app.Run();
        }
    }
}