using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using DashBoard.Areas.Identity;
using DashBoard.Data;
using Radzen;
using DashBoard.Areas.Identity.Data;
using DashBoard.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Configuration.AddJsonFile("secrets.json");
    
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
 
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 4;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider,
    RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();


builder.Services.AddTransient<IAddUser, RAddUser>();
builder.Services.AddTransient<IEnviarMail, REnviarMail>();

builder.Services.AddTransient<Repo<ZConfig, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z100_Org, ApplicationDbContext>>();

//builder.Services.AddTransient<Repo<Z104_Domicilio, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z110_User, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z170_File, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z180_EmpActiva, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z190_Bitacora, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z192_Logs, ApplicationDbContext>>();

builder.Services.AddTransient<Repo<Z200_Folio, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z201_FolioPrint, ApplicationDbContext>>();
/*
builder.Services.AddTransient<Repo<Z203_Transporte, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z204_Empleado, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z205_Carro, ApplicationDbContext>>();
*/
builder.Services.AddTransient<Repo<Z209_Campos, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z210_Concepto, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z220_Factura, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z222_FactDet, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z230_Pago, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z232_PagoDet, ApplicationDbContext>>();
builder.Services.AddTransient<Repo<Z280_Producto, ApplicationDbContext>>();
//builder.Services.AddTransient<Repo<Z232_EmpAlta, ApplicationDbContext>>();
//builder.Services.AddTransient<Repo<Z300_RUno, ApplicationDbContext>>();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

