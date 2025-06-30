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

// Configure gRPC from appsettings.json
var grpcSection = builder.Configuration.GetSection("Grpc");
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = grpcSection.GetValue<bool>("EnableDetailedErrors");
    options.MaxReceiveMessageSize = grpcSection.GetValue<int>("MaxReceiveMessageSize");
    options.MaxSendMessageSize = grpcSection.GetValue<int>("MaxSendMessageSize");
});

// Configure Kestrel from appsettings.json
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    var kestrelSection = context.Configuration.GetSection("Kestrel");
    serverOptions.Configure(kestrelSection);
});

var app = builder.Build();

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

app.Run();
