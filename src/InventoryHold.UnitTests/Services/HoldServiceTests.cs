using System;
using System.Threading.Tasks;
using InventoryHold.Contracts.DTOs;
using InventoryHold.Contracts.Events;
using InventoryHold.Domain.Entities;
using InventoryHold.Domain.Repositories;
using InventoryHold.Domain.Services;
using Moq;
using NUnit.Framework;

namespace InventoryHold.UnitTests.Services;

public class HoldServiceTests
{
    [Test]
    public async Task CreateHold_Succeeds_WhenStockAvailable()
    {
        var inv = new Mock<IInventoryRepository>();
        inv.Setup(x => x.TryReserve("p1", 2)).ReturnsAsync(true);
        var holdRepo = new Mock<IHoldRepository>();
        holdRepo.Setup(x => x.Create(It.IsAny<Hold>())).ReturnsAsync((Hold h) => { h.Id = "h1"; return h; });
        var pub = new Mock<IMessagePublisher>();

        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        var res = await svc.CreateHold(new HoldRequest { ProductId = "p1", Quantity = 2 });

        Assert.AreEqual("p1", res.ProductId);
        Assert.AreEqual(2, res.Quantity);
        Assert.IsNotNull(res.HoldId);
        pub.Verify(p => p.Publish(It.IsAny<HoldCreated>()), Times.Once);
    }

    [Test]
    public void CreateHold_Fails_WhenStockInsufficient()
    {
        var inv = new Mock<IInventoryRepository>();
        inv.Setup(x => x.TryReserve("p1", 2)).ReturnsAsync(false);
        var holdRepo = new Mock<IHoldRepository>();
        var pub = new Mock<IMessagePublisher>();

        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        Assert.ThrowsAsync<InvalidOperationException>(async () => await svc.CreateHold(new HoldRequest { ProductId = "p1", Quantity = 2 }));
        pub.Verify(p => p.Publish(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public async Task GetHold_ExpiresAndPublishes_WhenExpired()
    {
        var inv = new Mock<IInventoryRepository>();
        var holdRepo = new Mock<IHoldRepository>();
        var existed = new Hold { Id = "h1", ProductId = "p1", Quantity = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(-1), Released = false };
        holdRepo.Setup(x => x.Get("h1")).ReturnsAsync(existed);
        holdRepo.Setup(x => x.Update(It.IsAny<Hold>())).ReturnsAsync((Hold h) => h);
        var pub = new Mock<IMessagePublisher>();

        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        var res = await svc.GetHold("h1");

        Assert.AreEqual("h1", res.HoldId);
        pub.Verify(p => p.Publish(It.IsAny<HoldExpired>()), Times.Once);
        inv.Verify(i => i.Release("p1", 1), Times.Once);
    }

    [Test]
    public async Task ReleaseHold_ReleasesInventoryAndPublishes()
    {
        var inv = new Mock<IInventoryRepository>();
        var holdRepo = new Mock<IHoldRepository>();
        var existed = new Hold { Id = "h2", ProductId = "p2", Quantity = 3, ExpiresAt = DateTime.UtcNow.AddMinutes(10), Released = false };
        holdRepo.Setup(x => x.Get("h2")).ReturnsAsync(existed);
        holdRepo.Setup(x => x.Update(It.IsAny<Hold>())).ReturnsAsync((Hold h) => h);
        var pub = new Mock<IMessagePublisher>();

        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        await svc.ReleaseHold("h2");

        inv.Verify(i => i.Release("p2", 3), Times.Once);
        pub.Verify(p => p.Publish(It.IsAny<HoldReleased>()), Times.Once);
    }

    [Test]
    public void ReleaseHold_Throws_WhenNotFound()
    {
        var inv = new Mock<IInventoryRepository>();
        var holdRepo = new Mock<IHoldRepository>();
        holdRepo.Setup(x => x.Get("x")).ReturnsAsync((Hold)null);
        var pub = new Mock<IMessagePublisher>();
        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        Assert.ThrowsAsync<KeyNotFoundException>(async () => await svc.ReleaseHold("x"));
    }

    [Test]
    public async Task GetHold_ReturnsNull_WhenNotFound()
    {
        var inv = new Mock<IInventoryRepository>();
        var holdRepo = new Mock<IHoldRepository>();
        holdRepo.Setup(x => x.Get("nope")).ReturnsAsync((Hold)null);
        var pub = new Mock<IMessagePublisher>();

        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        var res = await svc.GetHold("nope");
        Assert.IsNull(res);
        pub.Verify(p => p.Publish(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public async Task ReleaseHold_NoOp_WhenAlreadyReleased()
    {
        var inv = new Mock<IInventoryRepository>();
        var holdRepo = new Mock<IHoldRepository>();
        var existed = new Hold { Id = "h3", ProductId = "p3", Quantity = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(10), Released = true };
        holdRepo.Setup(x => x.Get("h3")).ReturnsAsync(existed);
        var pub = new Mock<IMessagePublisher>();

        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        // should not throw and should not call release or publish
        await svc.ReleaseHold("h3");
        inv.Verify(i => i.Release(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        pub.Verify(p => p.Publish(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public async Task Concurrent_CreateHold_OnlyOneSucceeds()
    {
        var inv = new Mock<IInventoryRepository>();
        // first call reserves, second call fails
        inv.SetupSequence(x => x.TryReserve("p1", 1)).ReturnsAsync(true).ReturnsAsync(false);

        var holdRepo = new Mock<IHoldRepository>();
        holdRepo.Setup(x => x.Create(It.IsAny<Hold>())).ReturnsAsync((Hold h) => { h.Id = Guid.NewGuid().ToString(); return h; });
        var pub = new Mock<IMessagePublisher>();

        var svc = new HoldService(inv.Object, holdRepo.Object, pub.Object, TimeSpan.FromMinutes(5));

        var tasks = new[] {
            Task.Run(async () => { try { await svc.CreateHold(new HoldRequest { ProductId = "p1", Quantity = 1 }); return true; } catch { return false; } }),
            Task.Run(async () => { try { await svc.CreateHold(new HoldRequest { ProductId = "p1", Quantity = 1 }); return true; } catch { return false; } })
        };

        await Task.WhenAll(tasks);

        // exactly one should succeed
        var successCount = tasks.Count(t => t.Result);
        Assert.AreEqual(1, successCount);
        pub.Verify(p => p.Publish(It.IsAny<HoldCreated>()), Times.Once);
        holdRepo.Verify(h => h.Create(It.IsAny<Hold>()), Times.Once);
        inv.Verify(i => i.TryReserve("p1", 1), Times.Exactly(2));
    }
}
