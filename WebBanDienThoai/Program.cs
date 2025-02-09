using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using WebBanDienThoai.Models;
using WebBanDienThoai.Repository;
var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("BtlLtwQlbdtContext");
builder.Services.AddDbContext<BtlLtwQlbdtContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddScoped<IHangSpRepository, HangSpRepository>();

builder.Services.AddSession();

//Configuration Login Google Account
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;


//}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
//{
//    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
//    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
//});

// OpenID Connect -------------------------
// Đọc cấu hình từ appsettings.json
var authority = builder.Configuration["OpenIDConnect:Authority"];
var clientId = builder.Configuration["OpenIDConnect:ClientId"];
var clientSecret = builder.Configuration["OpenIDConnect:ClientSecret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie(options =>
{
    options.LoginPath = "/Access/Login";
    options.LogoutPath = "/Access/Logout";
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = authority;
    options.ClientId = clientId;
    options.ClientSecret = clientSecret;

    options.ResponseType = "code";
    options.SaveTokens = true;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // Cấu hình callback URL
    options.CallbackPath = "/signin-oidc";

    options.GetClaimsFromUserInfoEndpoint = true;

    // Thêm các events để debug
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            // Log URL redirect
            var redirectUrl = context.ProtocolMessage.RedirectUri;
            Console.WriteLine($"Redirecting to: {redirectUrl}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.Response.Redirect("/Home/Error");
            context.HandleResponse();
            return Task.CompletedTask;
        },
        OnTicketReceived = context =>
        {
            // Log successful authentication
            Console.WriteLine("Authentication successful!");
            return Task.CompletedTask;
        }
    };

    // Bỏ qua xác thực certificate trong development
    options.RequireHttpsMetadata = false; // Chỉ dùng trong development
});

// OpenID Connect -------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");
    //pattern: "{controller=Home}/{action=index}/{id?}");

app.Run();
