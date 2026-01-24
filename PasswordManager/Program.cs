using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Account.Register;
using PasswordManager.Application.Security;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.Login;
using PasswordManager.Infrastructure.ForgotPassword;
using PasswordManager.Infrastructure.Vault;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Models.Email;
using PasswordManager.Infrastructure.Register;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Infrastructure.Vault;
using PasswordManager.Application.Account.Email;
using PasswordManager.Application.Account.ForgotPassword;

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
            builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            builder.Services.AddScoped<ILoginService, LoginService>();
            builder.Services.AddScoped<IRegisterService,RegisterService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IVaultHomeService, VaultService>();
            builder.Services.AddScoped<IVaultSidebarService, VaultService>();
            builder.Services.AddScoped<IResetPasswordService, PasswordResetService>();
            builder.Services.AddScoped<IVaultSettingsService, VaultService>();
            builder.Services.AddScoped<IGetItemService, GetItemService>();
            builder.Services.AddScoped<IUpdateItemFieldService, UpdateItemFieldService>();
            builder.Services.AddScoped<IDeleteItemService, DeleteItemService>();
            builder.Services.AddScoped<IAddItemService, AddItemService>();

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Welcome";
                    options.AccessDeniedPath = "/Welcome";
                    options.LogoutPath = "/Account/Logout"; 

                    options.ExpireTimeSpan = TimeSpan.FromMinutes(200); ///
                    options.SlidingExpiration = false;

                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });



            var app = builder.Build();

            // Configure the HTTP request pipeline
            /*if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }*/
            app.UseExceptionHandler("/Error");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHsts();


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", context =>
            {
                if (context.User.Identity?.IsAuthenticated == true)
                    context.Response.Redirect("/Vault/Home");
                else
                    context.Response.Redirect("/Welcome");

                return Task.CompletedTask;
            });

            // Configure routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Welcome}/{action=IndexWelcome}/{id?}");

            app.Run();
        }
    }
}
