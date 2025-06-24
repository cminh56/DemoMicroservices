var builder = WebApplication.CreateBuilder(args);

// Thêm YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Middleware pipeline
app.MapReverseProxy(); // Bắt buộc phải đặt trước các Map khác

// Thêm health endpoint
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();