using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using PasswordManager.Application.Account.Email;
using PasswordManager.Application.Account.ForgotPassword;
using PasswordManager.Application.Account.Login;
using PasswordManager.Application.Account.Register;
using PasswordManager.Application.Security;
using PasswordManager.Application.Vault;
using PasswordManager.Data;
using PasswordManager.Infrastructure.Email;
using PasswordManager.Infrastructure.ForgotPassword;
using PasswordManager.Infrastructure.Login;
using PasswordManager.Infrastructure.Register;
using PasswordManager.Infrastructure.Security;
using PasswordManager.Infrastructure.Vault;
using PasswordManager.Middleware;
using PasswordManager.Models.Email;
using PasswordManager.Infrastructure.Settings;

namespace PasswordManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings")
            );
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            builder.Services.AddScoped<ILoginService, LoginService>();
            builder.Services.AddScoped<IRegisterService, RegisterService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IVaultHomeService, VaultService>();
            builder.Services.AddScoped<IVaultSidebarService, VaultService>();
            builder.Services.AddScoped<IResetPasswordService, PasswordResetService>();
            builder.Services.AddScoped<IVaultSettingsService, VaultService>();
            builder.Services.AddScoped<IGetItemService, GetItemService>();
            builder.Services.AddScoped<IUpdateItemFieldService, UpdateItemFieldService>();
            builder.Services.AddScoped<IDeleteItemService, DeleteItemService>();
            builder.Services.AddScoped<IAddItemService, AddItemService>();
            builder.Services.AddScoped<IAddFolderService, AddFolderService>();
            builder.Services.AddScoped<IGetFolderService, GetFolderService>();
            builder.Services.AddScoped<IGetAllFoldersService, GetFolderService>();
            builder.Services.AddScoped<IDeleteFolderService, DeleteFolderService>();
            builder.Services.AddScoped<IEncryptionService, EncryptionService>();
            builder.Services.AddScoped<ISessionEncryptionService, SessionEncryptionService>();
            builder.Services.AddScoped<TokenService>();

            builder.Services.AddScoped<SettingsService>();

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Welcome";
                    options.AccessDeniedPath = "/Welcome";
                    options.LogoutPath = "/Account/Logout";

                    options.ExpireTimeSpan = TimeSpan.FromHours(3);
                    options.SlidingExpiration = true;

                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });


            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(3);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.", token);
                };

                // Login: 10 attempts per minute per IP
                options.AddFixedWindowLimiter("login", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 10;
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                // Register / ForgotPassword: 5 attempts per minute per IP
                options.AddFixedWindowLimiter("auth", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 5;
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });

            var app = builder.Build();

            // Auto-run migrations on startup (works inside Docker container)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

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

            app.UseSession();

            app.UseExceptionHandler("/Error");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self';";
                await next();
            });

            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<SessionAndEncryptionValidationMiddleware>();

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
