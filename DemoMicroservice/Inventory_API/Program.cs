using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Inventory_API.Infastructure.DataContext;
using Inventory_API.Domain.Interfaces;
using Inventory_API.Infastructure.Repositories;
using Inventory_API.Application.Services;
using Inventory_API.Infastructure.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("InventoryDB")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Repository
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

// Add Services
builder.Services.AddScoped<InventoryService>();

// Add gRPC
builder.Services.AddGrpc();

// Configure Kestrel to support gRPC with HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    // Configure HTTP/2 endpoint for gRPC
    options.ListenAnyIP(81, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
    
    // Configure HTTP/1.1 endpoint for health checks
    options.ListenAnyIP(80, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

// Configure gRPC
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});

var app = builder.Build();

// Auto migrate database on startup
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
//    db.Database.Migrate();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Map gRPC service
app.MapGrpcService<InventoryGrpcService>();

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
