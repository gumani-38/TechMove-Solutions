namespace Gumani_Moila_EAPD7111w_POE.Models
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Session.GetString("AuthToken");
            var path = context.Request.Path.Value?.ToLower();

            // Allow login/logout routes without token
            if (string.IsNullOrEmpty(token) && !path.Contains("auth/login"))
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            await _next(context);
        }
    }
}
