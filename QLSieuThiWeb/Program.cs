using Microsoft.EntityFrameworkCore;
using QLSieuThiWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Thêm cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký QLSieuThiWebContext với DI container
builder.Services.AddDbContext<QLSieuThiWebContext>(options =>
    options.UseSqlServer("Server=QUANGION;Database=TT3;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"));

builder.WebHost.UseUrls("http://0.0.0.0:5062");
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
}); // Thêm dòng này để kích hoạt API controllers

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thêm middleware Session trước UseAuthorization
app.UseSession();

app.UseAuthentication();  // Thêm dòng này để kích hoạt xác thực nếu dùng xác thực trong ứng dụng

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "account",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers(); // Thêm dòng này để map các API controllers

app.Run();
