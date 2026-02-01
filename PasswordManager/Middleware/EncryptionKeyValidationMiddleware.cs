using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using PasswordManager.Application.Security;
using System.Security.Claims;

namespace PasswordManager.Middleware
{
    public class EncryptionKeyValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public EncryptionKeyValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
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

                byte[]? key = sessionEncryption.GetEncryptionKey(userId.Value);

                if (key == null)
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            await _next(context);
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