using StackExchange.Redis;
using Basket_API.Application.Services;
using Basket_API.Infastructure.Repositories;
using Basket_API.Domain.Interfaces;
using Basket_API.Infastructure.Mappings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis")["ConnectionString"];
    return ConnectionMultiplexer.Connect(configuration);
});

// Đăng ký BasketService
builder.Services.AddScoped<BasketService>();

// Đăng ký BasketRepository
builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// Đăng ký BasketCheckoutPublisher
builder.Services.AddScoped<BasketCheckoutPublisher>();

// Đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();


