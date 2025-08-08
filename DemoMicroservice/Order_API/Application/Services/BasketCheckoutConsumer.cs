using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Impl;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Order_API.Application.Services;

public class BasketCheckoutConsumer : BackgroundService
{
    private readonly string _hostname;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _queueName = "basket-checkout";
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BasketCheckoutConsumer> _logger;

    public BasketCheckoutConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<BasketCheckoutConsumer> logger)
    {
        var section = configuration.GetSection("RabbitMQ");
        _hostname = section["Host"] ?? "rabbitmq";
        _port = int.Parse(section["Port"] ?? "5672");
        _username = section["UserName"] ?? "guest";
        _password = section["Password"] ?? "guest";
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() 
        { 
            HostName = _hostname, 
            Port = _port, 
            UserName = _username, 
            Password = _password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        IConnection? connection = null;
        IModel? channel = null;
        
        // Retry connection logic
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to RabbitMQ at {HostName}:{Port}", _hostname, _port);
                
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
 
                channel.QueueDeclare(
                    queue: _queueName, 
                    durable: true, 
                    exclusive: false, 
                    autoDelete: false, 
                    arguments: null);
                    
                _logger.LogInformation("Successfully connected to RabbitMQ and listening on queue: {QueueName}", _queueName);
                break; // Successfully connected, exit retry loop
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Retrying in 10 seconds...");
                
                // Clean up any partial connections
                channel?.Dispose();
                connection?.Dispose();
                
                // Wait before retrying
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
        
        if (connection == null || channel == null)
        {
            _logger.LogError("Failed to establish connection to RabbitMQ after multiple attempts");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received checkout message: {Message}", message);

            try
            {
                var basketCheckout = JsonSerializer.Deserialize<BasketCheckoutMessage>(message);
                if (basketCheckout == null)
                {
                    _logger.LogError("Cannot deserialize basket checkout message: {Message}", message);
                    return;
                }
                if (basketCheckout.Items == null || !basketCheckout.Items.Any())
                {
                    _logger.LogWarning("Basket checkout message has no items: {Message}", message);
                    return;
                }
                _logger.LogInformation("BasketCheckoutMessage has {Count} items", basketCheckout.Items.Count);
                using var scope = _serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

                // 1. Tạo Order trước
                if (!Guid.TryParse(basketCheckout.UserId, out Guid userId))
                {
                    _logger.LogError("Invalid UserId format: {UserId}", basketCheckout.UserId);
                    return;
                }

                var order = new Order_API.Domain.Entities.Order
                {
                    Id = Guid.NewGuid(),
                    UserID = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Created",
                    PaymentMethod = basketCheckout.PaymentMethod ?? "CreditCard",
                    OrderDetails = new List<Order_API.Domain.Entities.OrderDetail>()
                };
                order.TotalAmount = 0;
                try
                {
                    await orderService.AddAsync(order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create Order for message: {Message}", message);
                    return;
                }

                bool allDetailSuccess = true;
                foreach (var item in basketCheckout.Items)
                {
                    _logger.LogInformation("Processing OrderDetail for ProductId: {ProductId}, Quantity: {Quantity}", item.ProductId, item.Quantity);
                    var detail = new Order_API.Domain.Entities.OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = 0 // TODO: lấy giá thực tế từ Product API nếu cần
                    };
                    try
                    {
                        await orderService.AddOrderDetailAsync(detail);
                        order.TotalAmount += detail.Price * detail.Quantity;
                        order.OrderDetails.Add(detail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create OrderDetail for Order {OrderId}, Product {ProductId}", order.Id, item.ProductId);
                        allDetailSuccess = false;
                        break;
                    }
                }

                if (!allDetailSuccess)
                {
                    // Xóa order đã tạo nếu có lỗi khi tạo order detail
                    try
                    {
                        await orderService.RemoveAsync(order.Id);
                        _logger.LogWarning("Order {OrderId} has been removed due to OrderDetail creation failure.", order.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to remove Order {OrderId} after OrderDetail creation failure.", order.Id);
                    }
                    return;
                }

                // 3. Cập nhật lại tổng tiền cho Order nếu cần
                try
                {
                    order.TotalAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);
                    await orderService.UpdateAsync(order.Id, order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update Order total amount for Order {OrderId}", order.Id);
                }

                _logger.LogInformation("Order created and saved for message: {Message}", message);
                channel.BasicAck(ea.DeliveryTag, false);
                return;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while processing checkout message");
                channel.BasicNack(ea.DeliveryTag, false, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout message");
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicQos(0, 1, false); // Process one message at a time
        channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        
        // Keep the service running until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
        
        // Clean up resources
        channel?.Dispose();
        connection?.Dispose();
    }
}

// Helper classes outside the BasketCheckoutConsumer class, but inside the namespace
internal class BasketCheckoutMessage
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("items")]
    public List<BasketCheckoutItem> Items { get; set; } = new();
}
internal class BasketCheckoutItem
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
} 