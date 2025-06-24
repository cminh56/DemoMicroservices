using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

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
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            Port = _port,
            UserName = _username,
            Password = _password
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received checkout message: {Message}", message);

            // Deserialize message và tạo Order
            using var scope = _serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
            // TODO: Deserialize message thành DTO phù hợp, ví dụ BasketCheckoutDTO
            // var basketCheckout = JsonSerializer.Deserialize<BasketCheckoutDTO>(message);
            // TODO: Map sang Order, gọi orderService.AddAsync(...)
            _logger.LogInformation("Order created for message: {Message}", message);
        };
        channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
} 