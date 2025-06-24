using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Basket_API.Application.Services;

public class BasketCheckoutPublisher
{
    private readonly string _hostname;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _queueName = "basket-checkout";

    public BasketCheckoutPublisher(IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMQ");
        _hostname = section["Host"] ?? "rabbitmq";
        _port = int.Parse(section["Port"] ?? "5672");
        _username = section["UserName"] ?? "guest";
        _password = section["Password"] ?? "guest";
    }

    public void Publish(object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            Port = _port,
            UserName = _username,
            Password = _password
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
    }
}