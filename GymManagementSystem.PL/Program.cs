using GymManagementSystem.BLL.Mapping;
using GymManagementSystem.BLL.Services.Classes;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Classes;
using GymManagementSystem.DAL.Repositories.Interfaces;
using GymManagementSystem.PL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. DbContext Configuration
builder.Services.AddDbContext<GymDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 2. Identity Services & Advanced Configuration (????? ?????? ?????? ???? ??????)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(Config =>
{
    Config.User.RequireUniqueEmail = true;
    Config.Lockout.MaxFailedAccessAttempts = 5; // ????? ??? 5 ??????? ?????
    Config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2); // ??? ????? ???????
})
.AddEntityFrameworkStores<GymDbContext>()
.AddDefaultTokenProviders();

// 3. Configure Application Cookie (????? ???????? ??? ?????? ????? ??????)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; // ???? ?? ????? ????? ??
});

// 4. Dependency Injection (Repositories & UnitOfWork)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// 5. Dependency Injection (Services)
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMemberShipService, MemberShipService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();

// 6. AutoMapper Configuration
builder.Services.AddAutoMapper(M => M.AddProfile(new MappingProfiles()));

var app = builder.Build();

// ==========================================
// ??? ????? ??? Migrations ???? Data Seeding ???????? ??? ???????
// ==========================================
await app.MigrateAndSeedAsync();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ???? ??????? ???????
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// 7. Map Controller Route (????? ??? Default Route ????? ???? ?????? ?? ??????? ???????? ??????)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();