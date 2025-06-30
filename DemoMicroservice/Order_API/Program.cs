using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order_API.Infastructure.DataContext;
using Order_API.Domain.Interfaces;
using Order_API.Infastructure.Repositories;
using Order_API.Application.Services;
using AutoMapper;
using Order_API.Protos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();
app.MapControllers();


app.Run();
