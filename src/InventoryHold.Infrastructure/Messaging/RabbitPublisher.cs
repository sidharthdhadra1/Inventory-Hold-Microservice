using System.Text.Json;
using InventoryHold.Domain.Services;
using RabbitMQ.Client;

namespace InventoryHold.Infrastructure.Messaging;

public class RabbitPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _channel;
    private readonly string _exchange = "inventory-holds";

    public RabbitPublisher(string host, string user, string pass)
    {
        var factory = new ConnectionFactory { HostName = host, UserName = user, Password = pass };
        _conn = factory.CreateConnection();
        _channel = _conn.CreateModel();
        _channel.ExchangeDeclare(_exchange, ExchangeType.Fanout, durable: true);
    }

    public Task Publish<T>(T evt)
    {
        var json = JsonSerializer.Serialize(evt);
        var body = System.Text.Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(_exchange, "", null, body);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _conn?.Dispose();
    }
}
