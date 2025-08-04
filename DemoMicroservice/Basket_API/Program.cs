using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Basket_API.Application.Services;
using Basket_API.Infastructure.Repositories;
using Basket_API.Domain.Interfaces;
using Basket_API.Infastructure.Mappings;

var builder = WebApplication.CreateBuilder(args);
// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

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

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API V1");
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();
app.Run();


