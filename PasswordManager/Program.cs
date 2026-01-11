using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Models.Email;

namespace PasswordManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
                ));

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings")
            );
            builder.Services.AddScoped<EmailService>();

            // TODO: Add Authentication services
            // builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //     .AddCookie(options =>
            //     {
            //         options.LoginPath = "/Account/Login";
            //         options.LogoutPath = "/Account/Logout";
            //     });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // TODO: Enable authentication
            // app.UseAuthentication();
            app.UseAuthorization();

            // Configure routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Welcome}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
