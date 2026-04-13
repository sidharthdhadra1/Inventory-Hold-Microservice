using InventoryHold.Domain.Repositories;
using InventoryHold.Domain.Services;
using InventoryHold.Infrastructure.Config;
using InventoryHold.Infrastructure.Mongo;
using InventoryHold.Infrastructure.Messaging;
using InventoryHold.Infrastructure.Redis;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var mongoConn = Environment.GetEnvironmentVariable("MONGO_CONN") ?? builder.Configuration.GetConnectionString("Mongo") ?? "mongodb://localhost:27017";
var redisConn = Environment.GetEnvironmentVariable("REDIS_CONN") ?? "localhost:6379";
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";

var useEf = (Environment.GetEnvironmentVariable("USE_EF") ?? "false").ToLower() == "true";
var db = MongoConfig.Create(mongoConn);

builder.Services.AddSingleton(db); // always register Mongo DB for holds
builder.Services.AddSingleton<ICacheService>(sp => new RedisCacheService(redisConn));

if (useEf)
{
    // configure EF Core with SQLite for demo local development
    builder.Services.AddDbContext<InventoryHold.Infrastructure.EF.InventoryDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=inventory.db"));
    builder.Services.AddScoped<IInventoryRepository, InventoryHold.Infrastructure.EF.EfInventoryRepository>();
}
else
{
    builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
}

// Holds are still stored in Mongo
builder.Services.AddSingleton<IHoldRepository, HoldRepository>();

builder.Services.AddSingleton<IMessagePublisher>(sp => new RabbitPublisher(rabbitHost, rabbitUser, rabbitPass));
builder.Services.AddSingleton<HoldService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Inventory Hold API",
        Version = "v1",
        Description = "API for creating and managing inventory holds"
    });

    // try to include XML comments when available
    var xmlFile = System.IO.Path.ChangeExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location ?? "", ".xml");
    if (System.IO.File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

var app = builder.Build();

// middleware
app.UseMiddleware<InventoryHold.WebApi.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<InventoryHold.WebApi.Middleware.RequestLoggingMiddleware>();

// seed data (only for mongo path)
try
{
    if (!useEf)
    {
        await SeedData.EnsureSeed(db);
    }
    else
    {
        // seed EF DB
        using var scope = app.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<InventoryHold.Infrastructure.EF.InventoryDbContext>();
        await InventoryHold.Infrastructure.EF.SeedData.EnsureSeed(ctx);
    }
}
catch (MongoDB.Driver.MongoConnectionException mex)
{
    var logger = app.Services.GetService<ILogger<Program>>();
    logger?.LogWarning(mex, "Could not connect to MongoDB during startup seed. The application will continue and retry operations at runtime.");
}
catch (Exception ex)
{
    var logger = app.Services.GetService<ILogger<Program>>();
    logger?.LogError(ex, "Unexpected error during startup seeding. The application will continue.");
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
