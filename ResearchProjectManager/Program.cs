using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ResearchProjectManager.Data;
using ResearchProjectManager.Models;
using ResearchProjectManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ProjectTaskService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<SubtaskService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<AttachmentService>();
builder.Services.AddScoped<TaskAssignmentService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddHostedService<ResearchProjectManager.Data.DbSeeder>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//TODO: remake the restrictions for the password, because it is only for testing purposes as of now
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;

})
.AddRoles<Role>()
.AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
