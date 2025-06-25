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

// Configure gRPC client for InventoryService
builder.Services.AddGrpcClient<InventoryService.InventoryServiceClient>(o =>
{
    o.Address = new Uri("http://inventory-api:81");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// Configure Kestrel to support both HTTP/1.1 and HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(lo =>
    {
        lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});


builder.Services.AddHostedService<BasketCheckoutConsumer>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
