using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PasswordManager.Application.Security;
using PasswordManager.Data;
using System.Security.Claims;

namespace PasswordManager.Middleware
{
    public class SessionAndEncryptionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAndEncryptionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            if (IsPublicRoute(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var userId = GetUserIdFromClaims(context);
            if (userId.HasValue)
            {
                var sessionEncryption = context.RequestServices
                    .GetRequiredService<ISessionEncryptionService>();
                byte[]? encryptionKey = sessionEncryption.GetEncryptionKey(userId.Value);

                if (encryptionKey == null)
                {
                    await LogoutUser(context, userId.Value);
                    context.Response.Redirect("/Account/Login?reason=nokey");
                    return;
                }

                var lastActivityKey = "LastActivity";
                var lastActivityBytes = context.Session.Get(lastActivityKey);

                if (lastActivityBytes != null)
                {
                    var lastActivity = DateTime.FromBinary(BitConverter.ToInt64(lastActivityBytes, 0));
                    var user = await db.Users.FindAsync(userId.Value);
                    var timeoutMinutes = user?.SessionTimeoutMinutes ?? 30;

                    if (DateTime.UtcNow - lastActivity > TimeSpan.FromMinutes(timeoutMinutes))
                    {
                        await LogoutUser(context, userId.Value);
                        context.Response.Redirect("/Account/Login?reason=timeout");
                        return;
                    }
                }

                var nowBytes = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                context.Session.Set(lastActivityKey, nowBytes);
            }

            await _next(context);
        }

        private async Task LogoutUser(HttpContext context, int userId)
        {
            context.Session.Remove($"EncryptionKey_{userId}");

            context.Session.Clear();

            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private static int? GetUserIdFromClaims(HttpContext context)
        {
            var claim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
                return userId;
            return null;
        }

        private static bool IsPublicRoute(PathString path)
        {
            return path.StartsWithSegments("/Account")
                || path.StartsWithSegments("/Welcome")
                || path.StartsWithSegments("/Error")
                || path.StartsWithSegments("/favicon.ico")
                || path.StartsWithSegments("/css")
                || path.StartsWithSegments("/js")
                || path.StartsWithSegments("/images");
        }
    }
}