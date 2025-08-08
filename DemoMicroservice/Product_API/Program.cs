using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product_API.Application.Services;
using Product_API.Domain.Interfaces;
using Product_API.Infastructure.DataContext;
using Product_API.Infastructure.Mappings;
using Product_API.Infastructure.Repositories;
using System.Text;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProductDB"));
});

// Add repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Add services
builder.Services.AddScoped<ProductService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
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
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add HttpClient for CategoryHttpClientService
builder.Services.AddHttpClient<CategoryHttpClientService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CatalogApiBaseUrl"]);
});

// Add InventoryGrpcClientService with direct gRPC client
builder.Services.AddSingleton<InventoryGrpcClientService>();

// Configure gRPC client manually
builder.Services.AddSingleton(services =>
{
    var inventoryGrpcServiceUrl = builder.Configuration["Services:InventoryGrpcService"];
    if (string.IsNullOrEmpty(inventoryGrpcServiceUrl))
    {
        throw new ArgumentException("Inventory gRPC service URL is not configured");
    }
    
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    
    var channel = GrpcChannel.ForAddress(inventoryGrpcServiceUrl, new GrpcChannelOptions
    {
        HttpHandler = handler
    });
    
    return new Inventory_API.Protos.InventoryService.InventoryServiceClient(channel);
});

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
