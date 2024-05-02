using Microsoft.EntityFrameworkCore;
using ResumeCheckSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ResumeCheckDBContext>(options =>
    options.UseSqlServer("Server=(local)\\sqlexpress;Database=ResumeCheckDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
);

// Add sessions
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ResumeCheckSystem.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Adjust as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

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

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // dont forget to change to Home

app.Run();
