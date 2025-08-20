using Practicas_Demo1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Servicios propios
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RequesitionService>();
builder.Services.AddScoped<OfficeSupService>();
builder.Services.AddScoped<IFolioService, FolioService>();
builder.Services.AddScoped<FolioVerificacionService>();
builder.Services.AddScoped<MaterialsService>();
builder.Services.AddScoped<ToolsService>();
builder.Services.AddScoped<DashboardService>();


// Soporte para HttpContext y sesiones
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de inactividad
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Necesario para que siempre funcione
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Employee/Login");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Sesión debe ir después de UseRouting
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Login}/{id?}");

app.Run();
