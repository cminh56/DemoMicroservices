using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672, UserName = "guest", Password = "guest" };
        IConnection connection = null;
        int retry = 0;
        while (connection == null && retry < 10)
        {
            try
            {
                connection = factory.CreateConnection();
            }
            catch
            {
                retry++;
                Thread.Sleep(3000); // đợi 3s rồi thử lại
            }
        }
        if (connection == null)
            throw new Exception("Could not connect to RabbitMQ after 10 retries");
        var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
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
                var order = new Order_API.Domain.Entities.Order
                {
                    Id = Guid.NewGuid(),
                    UserID = basketCheckout.UserId,
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing basket checkout message: {Message}", message);
            }
        };
        channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}

// Thêm class tạm cho deserialize message
internal class BasketCheckoutMessage
{
    public Guid UserId { get; set; }
    
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