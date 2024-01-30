using Microsoft.EntityFrameworkCore;
using MyApp.DataAccessLayer;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.DataAccessLayer.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using MyApp.CommonHelper;
using Stripe;

using MyApp.DataAccessLayer.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddDbContext<ApplicatonDbContext>(option => option.UseSqlServer(
    
    builder.Configuration.GetConnectionString("DefaultConnection")
    
    ));
// Add services to the stripe to test payment 
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("PaymentSettings"));

/// <summary>
/// Add Identity
/// </summary>
builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicatonDbContext>();

//Here add IEmailSender service add

builder.Services.AddSingleton<IEmailSender, EmailSender>();

//Here Razor page service add
builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(option =>
{
    option.AccessDeniedPath = "/Identity/Account/AccessDenied";
    option.LoginPath= "/Identity/Account/Login";
    option.LogoutPath = "/Identity/Account/Logout";

});
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
StripeConfiguration.ApiKey = builder.Configuration.GetSection("PaymentSettings:Secretkey").Get<string>();
DataSedding();


app.UseAuthentication();
app.UseAuthorization();

//Maping for razor pages
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

void DataSedding()
{
    using(var scope=app.Services.CreateScope())
    {
        var Dbinitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        Dbinitializer.Initialize();
    }
}

