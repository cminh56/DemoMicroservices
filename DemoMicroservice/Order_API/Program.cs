using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Order_API.Infastructure.DataContext;
using Order_API.Domain.Interfaces;
using Order_API.Infastructure.Repositories;
using Order_API.Application.Services;
using AutoMapper;
using Order_API.Protos;

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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
    
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

// Đăng ký DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderDB")));

// Đăng ký repository và service
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<OrderService>();

// Đăng ký Inventory Client Service
builder.Services.AddScoped<IInventoryClientService, InventoryClientService>();

// Get gRPC configuration from appsettings.json
var grpcSection = builder.Configuration.GetSection("Grpc");
var inventoryGrpcUrl = builder.Configuration["InventoryService:GrpcUrl"];

// Configure gRPC client for InventoryService with settings from appsettings.json
builder.Services.AddGrpcClient<InventoryService.InventoryServiceClient>(options =>
{
    options.Address = new Uri(inventoryGrpcUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
})
.ConfigureChannel(grpcChannelOptions =>
{
    // Configure channel options from appsettings
    grpcChannelOptions.MaxReceiveMessageSize = grpcSection.GetValue<int>("MaxReceiveMessageSize");
    grpcChannelOptions.MaxSendMessageSize = grpcSection.GetValue<int>("MaxSendMessageSize");
    
    // Configure HTTP handler
    grpcChannelOptions.HttpHandler = new SocketsHttpHandler
    {
        EnableMultipleHttp2Connections = grpcSection.GetSection("HttpHandler").GetValue<bool>("EnableMultipleHttp2Connections"),
        SslOptions = new System.Net.Security.SslClientAuthenticationOptions
        {
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                                 System.Security.Authentication.SslProtocols.Tls13
        }
    };
});

// Configure Kestrel from appsettings.json if not in development
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel((context, serverOptions) =>
    {
        var kestrelSection = context.Configuration.GetSection("Kestrel");
        if (kestrelSection.Exists())
        {
            serverOptions.Configure(kestrelSection);
        }
    });
}

builder.Services.AddHostedService<BasketCheckoutConsumer>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
