using System.Threading.Tasks;

namespace InventoryHold.Domain.Services;

public interface IMessagePublisher
{
    Task Publish<T>(T evt);
}
