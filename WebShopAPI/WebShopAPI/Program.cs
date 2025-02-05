using Microsoft.EntityFrameworkCore;
using WebShopAPI.Services.Interfaces;
using WebShopAPI.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Configure services in the container
// Add distributed memory cache (required for sessions)
builder.Services.AddDistributedMemoryCache();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set timeout to 30 min
    options.Cookie.HttpOnly = true; // Prevents client-side access
    options.Cookie.IsEssential = true; // Required for GDPR compliance
    options.Cookie.SameSite = SameSiteMode.Lax; // Important for session persistence
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Allow both HTTP & HTTPS
});


// Add HTTP context accessor (required for session-based services)
builder.Services.AddHttpContextAccessor();

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
