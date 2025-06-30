using StackExchange.Redis;
using Basket_API.Application.Services;
using Basket_API.Infastructure.Repositories;
using Basket_API.Domain.Interfaces;
using Basket_API.Infastructure.Mappings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis")["ConnectionString"];
    var config = ConfigurationOptions.Parse(configuration);
    
    // Additional configuration
    config.AllowAdmin = true;
    config.ReconnectRetryPolicy = new LinearRetry(5000);
    config.ConnectTimeout = 5000;
    
    // Log connection attempt
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Connecting to Redis at {string.Join(",", config.EndPoints)}...");
    
    try
    {
        var redis = ConnectionMultiplexer.Connect(config);
        redis.ConnectionFailed += (sender, e) => 
            logger.LogError(e.Exception, "Redis connection failed");
        
        redis.ConnectionRestored += (sender, e) => 
            logger.LogInformation("Redis connection restored");
            
        return redis;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to Redis");
        throw;
    }
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


