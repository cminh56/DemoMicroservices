using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure rate limiting from appsettings.json
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting");
var globalConfig = rateLimitingConfig.GetSection("Global");
var apiEndpointsConfig = rateLimitingConfig.GetSection("ApiEndpoints");

// Add rate limiting services
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    // Global rate limiting policy
    rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = globalConfig.GetValue<bool>("AutoReplenishment"),
                PermitLimit = globalConfig.GetValue<int>("PermitLimit"),
                Window = TimeSpan.FromMinutes(globalConfig.GetValue<int>("WindowInMinutes"))
            }));

    // Specific endpoint rate limiting
    rateLimiterOptions.AddFixedWindowLimiter("api", options =>
    {
        options.PermitLimit = apiEndpointsConfig.GetValue<int>("PermitLimit");
        options.Window = TimeSpan.FromMinutes(apiEndpointsConfig.GetValue<int>("WindowInMinutes"));
        options.QueueLimit = apiEndpointsConfig.GetValue<int>("QueueLimit");
        options.QueueProcessingOrder = apiEndpointsConfig.GetValue<string>("QueueProcessingOrder") == "OldestFirst" 
            ? QueueProcessingOrder.OldestFirst 
            : QueueProcessingOrder.NewestFirst;
    });

    // Configure rate limiting response
    rateLimiterOptions.RejectionStatusCode = rateLimitingConfig.GetValue<int>("RejectionStatusCode");
});

// Thêm YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Thêm Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
    
    // Thêm mô tả về các services
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.RelativePath != null)
        {
            if (apiDesc.RelativePath.StartsWith("product"))
                return docName == "v1";
            if (apiDesc.RelativePath.StartsWith("order"))
                return docName == "v1";
            if (apiDesc.RelativePath.StartsWith("inventory"))
                return docName == "v1";
            if (apiDesc.RelativePath.StartsWith("basket"))
                return docName == "v1";
            if (apiDesc.RelativePath.StartsWith("catalog"))
                return docName == "v1";
        }
        return false;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = string.Empty; // Để Swagger UI là trang chủ
    });
}

// Apply rate limiting to the reverse proxy
app.UseRateLimiter();

// Apply rate limiting to specific routes
app.MapReverseProxy()
    .RequireRateLimiting("api"); // Apply the 'api' rate limiting policy

app.Run();