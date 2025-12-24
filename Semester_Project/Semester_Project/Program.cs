using ChatApp.Hubs;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;
using Semester_Project.Models.Interface;
using Semester_Project.Models.Repository;
using Semester_Project.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<myappuser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ISPuserinterface, Repository>();


builder.Services.AddSignalR();
builder.Services.AddTransient<IEmailSender, Semester_Project.Services.EmailSender>();
builder.Services.AddScoped<Semester_Project.Services.DashboardService>();
builder.Services.AddScoped<Semester_Project.Services.InvoiceService>();
builder.Services.AddScoped<Semester_Project.Services.INotificationService, Semester_Project.Services.NotificationService>();
builder.Services.AddHostedService<Semester_Project.Services.ExpirationCheckService>();



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => 
        policy.RequireRole("Admin"));

    options.AddPolicy("UserPolicy", policy => 
        policy.RequireRole("User"));
});



var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<myappuser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.Initialize(services, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Ensure Database Schema is up to date (Manual Migration for ReceiptPath)
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var dbConnString = config.GetConnectionString("DefaultConnection");
    
    try 
    {
        // Check for both "Data Source" and "DataSource"
        if (dbConnString != null && (dbConnString.Contains("Data Source") || dbConnString.Contains("DataSource")))
        {
            using (var connection = new SqliteConnection(dbConnString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('PaymentVerifications') WHERE name='ReceiptPath';";
                var exists = Convert.ToInt32(cmd.ExecuteScalar());
                
                if (exists == 0)
                {
                    cmd.CommandText = "ALTER TABLE PaymentVerifications ADD COLUMN ReceiptPath TEXT;";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("--> [SUCCESS] Added missing 'ReceiptPath' column to PaymentVerifications table.");
                }
                else 
                {
                    Console.WriteLine("--> [INFO] 'ReceiptPath' column already exists.");
                }
            }
        }
        else
        {
            Console.WriteLine("--> [WARNING] SQLITE connection string not recognized for manual migration.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> [ERROR] Database Migration failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<ChatHub>("/NOCHAT");

app.Run();
