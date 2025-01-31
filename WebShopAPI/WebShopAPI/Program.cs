using Microsoft.EntityFrameworkCore;
using WebShopAPI.Services.Interfaces;
using WebShopAPI.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add Session Support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor(); // Required for session-based services

// ✅ Register Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ✅ Register Other Services
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

// ✅ Add Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSession(); // Enable session middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
