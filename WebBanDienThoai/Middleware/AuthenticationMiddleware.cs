namespace WebBanDienThoai.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Cho phép truy cập các route không cần xác thực
            if (context.Request.Path.StartsWithSegments("/Access/Login") ||
                context.Request.Path.StartsWithSegments("/callback") ||
                context.Request.Path.StartsWithSegments("/Access/Logout"))
            {
                await _next(context);
                return;
            }

            // Kiểm tra xác thực cho các route khác
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.Redirect("/Access/Login");
                return;
            }

            await _next(context);
        }
    }

}
