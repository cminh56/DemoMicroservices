using Microsoft.EntityFrameworkCore;
using Product_API.Infastructure.DataContext;
using Microsoft.Extensions.DependencyInjection;
using Product_API.Application.Services;
using Product_API.Domain.Interfaces;
using Product_API.Infastructure.Repositories;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL DbContext
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProductDB")));

// Đăng ký repository và service
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddHttpClient<CategoryHttpClientService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CatalogApiBaseUrl"]);
});

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Auto migrate database on startup
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
//    db.Database.Migrate();
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));
app.Run();
