using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using EasyData.Services;
using AutoMapper;
using shoppingcartwebsite.Service;
using shoppingcartwebsite.Models;
using Microsoft.AspNetCore.Identity;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<DatabaseContext>();




builder.Services.AddDbContext<DatabaseContext>(options =>
         options.UseSqlServer(
             builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<DatabaseContext>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => //CookieAuthenticationOptions
    {
        options.LoginPath = new PathString("/Account/Authorization");
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<DataUpdateService>();

var emailConfig = builder.Configuration.GetSection("EmailConfiguration")
  .Get<EmailConfiguration>();

builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailPasswordSender, EmailPasswordSender>();

builder.Services.AddScoped<EmailSender>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
   opt.TokenLifespan = TimeSpan.FromHours(2));
builder.Services.AddSession();
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);



builder.Services.AddIdentity<User, ApplicationRole>(options =>
{

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 6;
    options.Lockout.AllowedForNewUsers = true;
    // User settings.
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;

})
          .AddEntityFrameworkStores<DatabaseContext>()
          .AddDefaultTokenProviders();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        //var dataUpdateService = services.GetRequiredService<DataUpdateService>();
        //dataUpdateService.UpData();
        var context = services.GetRequiredService<DatabaseContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        SeedData.InitializeAsync(context, userManager, roleManager).Wait();

    }
    catch (Exception ex)
    {
        // Handle exceptions
    }
}



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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
