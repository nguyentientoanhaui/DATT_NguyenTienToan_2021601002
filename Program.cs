using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Shopping_Demo.Areas.Admin.Repository;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;

var builder = WebApplication.CreateBuilder(args);

//Momo API Payment
builder.Services.Configure<MoMoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMoMoService, MoMoService>();
builder.Services.AddScoped<ILargePaymentService, LargePaymentService>();

//connection db
builder.Services.AddDbContext<DataContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb"));
});

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSingleton<IBadWordsService, BadWordsService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IDatabaseVietnamizationService, DatabaseVietnamizationService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});


builder.Services.AddIdentity<AppUserModel, IdentityRole>()
	.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

// Configure External Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var clientId = builder.Configuration["Authentication:Google:ClientId"];
        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        
        Console.WriteLine($"Google OAuth ClientId: {clientId}");
        Console.WriteLine($"Google OAuth ClientSecret: {(string.IsNullOrEmpty(clientSecret) ? "NOT SET" : "SET")}");
        
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        
        // Cấu hình quan trọng để hiển thị popup chọn tài khoản
        options.SaveTokens = true;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        
        // Thêm scope để lấy thông tin user
        options.Scope.Add("email");
        options.Scope.Add("profile");
        
        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnRemoteFailure = context =>
            {
                Console.WriteLine($"Google OAuth Remote Failure: {context.Failure?.Message}");
                context.HandleResponse();
                context.Response.Redirect("/Account/Login?error=google_auth_failed");
                return Task.CompletedTask;
            },
            OnCreatingTicket = context =>
            {
                Console.WriteLine($"Google OAuth Creating Ticket for user: {context.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    })
    .AddFacebook(options =>
    {
        var appId = builder.Configuration["Authentication:Facebook:AppId"];
        var appSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        
        Console.WriteLine($"Facebook OAuth AppId: {appId}");
        Console.WriteLine($"Facebook OAuth AppSecret: {(string.IsNullOrEmpty(appSecret) ? "NOT SET" : "SET")}");
        
        options.AppId = appId;
        options.AppSecret = appSecret;
    });

builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings.
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 4;

	options.User.RequireUniqueEmail = true;
});

var app = builder.Build();

app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "category",
    pattern: "/category/{Slug?}",
    defaults: new { controller="Category", action = "Index" });

app.MapControllerRoute(
    name: "brand",
    pattern: "/brand/{Slug?}",
    defaults: new { controller="Brand", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Seeding Data
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedingData(context);

app.Run();
