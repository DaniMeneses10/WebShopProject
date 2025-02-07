using Microsoft.EntityFrameworkCore;
using WebShopAPI.Services.Interfaces;
using WebShopAPI.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add distributed memory cache (required for sessions)
builder.Services.AddDistributedMemoryCache();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Add HTTP context accessor (required for session-based services)
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Permitir acceso a HttpContext


// Configure CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure database connection (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

// Add controllers and Swagger (API documentation)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure middleware in the correct order
app.UseHttpsRedirection();          
app.UseCors("AllowSpecificOrigin");
app.UseSession();
app.UseAuthorization();

app.MapControllers();
app.Run();
