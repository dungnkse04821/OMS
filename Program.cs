using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Endpoints;
using OMS.Models;
using OMS.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // ── Require authentication for all pages by default ──────────────
    options.Conventions.AuthorizeFolder("/");

    // ── Allow anonymous access to login/register pages ───────────────
    options.Conventions.AllowAnonymousToFolder("/Account");

    // ── Admin-only pages ─────────────────────────────────────────────
    options.Conventions.AuthorizeFolder("/AuditLog", "AdminOnly");

    // ── Admin + Staff can manage data (Viewer is read-only) ──────────
    options.Conventions.AuthorizePage("/Orders/Create", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Orders/Edit", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Orders/Delete", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Orders/ImportTracking", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Customers/Create", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Customers/Edit", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Customers/Delete", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Products/Create", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Products/Edit", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Products/Delete", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Carriers/Create", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Carriers/Edit", "StaffOrAdmin");
    options.Conventions.AuthorizePage("/Carriers/Delete", "StaffOrAdmin");
});

// Configure PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── ASP.NET Core Identity ────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings (relaxed for dev convenience)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie configuration ─────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ── Authorization policies ───────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(AppRoles.Admin));
    options.AddPolicy("StaffOrAdmin", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Staff));
});

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate and seed test data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        DbSeeder.Seed(context);

        // Seed Identity roles and default users
        await IdentitySeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Có lỗi xảy ra khi tự động migrate hoặc seed dữ liệu mẫu.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapApiEndpoints();
app.Run();
