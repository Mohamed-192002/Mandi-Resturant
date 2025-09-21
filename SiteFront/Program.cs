using Core.Interfaces;
using Infrastracture.Services;
using Infrastracture.Services.Permission;
using Infrastructure.Data;
using Infrastructure.Helper;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SiteFront.Hubs;
using SiteFront.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SiteDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthenticationServices();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

builder.Services.AddTransient<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddHttpContextAccessor();

builder.Services.AddMvc()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        ProgressBar = true,
        PositionClass = ToastPositions.TopCenter,
        PreventDuplicates = true,
        CloseButton = true,
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSignalR();

builder.Services.AddHttpClient<WhatsAppService>();
// Register the background service
builder.Services.AddHostedService<EndTimeCheckerService>();
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.Listen(IPAddress.Any, 5000);
//});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(IPAddress.Any, 5000);
    });
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//app.UseMiddleware<DeviceIdMiddleware>("BFEBFBFF000506E3-HTHJRF2"); // ProcessorId => wmic cpu get ProcessorId , SerialNumber => wmic bios get serialnumber from cmd
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapHub<DataHub>("/dataHub");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{area=Owner}/{controller=Product}/{action=Index}/{id?}"
//    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
