using KursachBD.Models.Enums;
using KursachBD.Pages;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Globalization;





AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddDbContext<KursachBD.Models.OnlineStoreSellingComputerEquipmentContext>(options =>
    options.UseNpgsql(
        "Server=localhost;Database=online_store_selling_computer_equipment;Username=Mikhail;Password=1_Mi7h9_1;Persist Security Info=True",
        npgsqlOptions =>
        {
            npgsqlOptions.MapEnum<KursachBD.Models.Enums.ProductType>("store.product_type_enum");
        })
);


builder.Services.AddScoped<CartService>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "KursachBD.Auth";
        options.LoginPath = "/Authoriz"; 
        options.LogoutPath = "/Authoriz?handler=Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddRazorPages();

var app = builder.Build();


var supportedCultures = new[] { new CultureInfo("en-US") }; 
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();  

app.UseStaticFiles();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
