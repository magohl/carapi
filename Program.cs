using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var carOrders = new List<CarOrder>();

// Root endpoint
app.MapGet("/", () => "Car Ordering API - Use /api/cars for CRUD operations");

// GET /api/cars - Get all car orders
app.MapGet("/api/cars", () => carOrders)
   .WithName("GetAllCarOrders")
   .WithTags("Cars");

// GET /api/cars/{id} - Get specific car order by ID
app.MapGet("/api/cars/{id:guid}", (Guid id) =>
{
    var order = carOrders.FirstOrDefault(c => c.Id == id);
    return order is not null ? Results.Ok(order) : Results.NotFound($"Car order with ID {id} not found");
})
.WithName("GetCarOrderById")
.WithTags("Cars");

// POST /api/cars - Create new car order
app.MapPost("/api/cars", (CreateCarOrderRequest request) =>
{
    var expectedDeliveryDate = DateTime
        .UtcNow.AddMonths(6)
        .AddDays(new Random().Next(-100, 101));

    var newOrder = new CarOrder(
        Id: Guid.NewGuid(),
        Make: request.Make,
        Model: request.Model,
        Color: request.Color,
        OrderDate: DateTime.UtcNow,
        ExpectedDeliveryDate: expectedDeliveryDate,
        Status: "Pending"
    );

    carOrders.Add(newOrder);

    return Results.Created($"/api/cars/{newOrder.Id}", newOrder);
})
.WithName("CreateCarOrder")
.WithTags("Cars");

// PUT /api/cars/{id} - Update existing car order
app.MapPut("/api/cars/{id:guid}", (Guid id, UpdateCarOrderRequest request) =>
{
    var existingOrder = carOrders.FirstOrDefault(c => c.Id == id);
    if (existingOrder is null)
    {
        return Results.NotFound($"Car order with ID {id} not found");
    }

    var updatedOrder = existingOrder with
    {
        Make = request.Make ?? existingOrder.Make,
        Model = request.Model ?? existingOrder.Model,
        Color = request.Color ?? existingOrder.Color,
        Status = request.Status ?? existingOrder.Status
    };

    var index = carOrders.FindIndex(c => c.Id == id);
    carOrders[index] = updatedOrder;

    return Results.Ok(updatedOrder);
})
.WithName("UpdateCarOrder")
.WithTags("Cars");

// DELETE /api/cars/{id} - Delete car order
app.MapDelete("/api/cars/{id:guid}", (Guid id) =>
{
    var order = carOrders.FirstOrDefault(c => c.Id == id);
    if (order is null)
    {
        return Results.NotFound($"Car order with ID {id} not found");
    }

    carOrders.Remove(order);
    return Results.NoContent();
})
.WithName("DeleteCarOrder")
.WithTags("Cars");

// GET /api/cars/makes - Get available car makes
app.MapGet("/api/cars/makes", () => new[]
{
    "Toyota", "BMW"
})
.WithName("GetAvailableMakes")
.WithTags("Cars");

// GET /api/cars/models/{make} - Get models for a specific make
app.MapGet("/api/cars/models/{make}", (string make) =>
{
    var models = make.ToLower() switch
    {
        "toyota" => new[] { "Camry", "Corolla", "RAV4", "Highlander", "Prius" },
        "bmw" => new[] { "3 Series", "5 Series", "X3", "X5", "M3" },
        _ => null
    };

    return models is not null ?
        Results.Ok(models)
            : Results.NotFound($"No models found for make '{make}'");
})
.WithName("GetModelsForMake")
.WithTags("Cars");

// GET /api/cars/colors - Get available colors
app.MapGet("/api/cars/colors", () => new[]
{
    "Black", "White", "Red", "Purple"
})
.WithName("GetAvailableColors")
.WithTags("Cars");

app.Run();


// Models
record Car(string Make, string Model, string Color);

record CarOrder(Guid Id, string Make, string Model, string Color, DateTime OrderDate, DateTime ExpectedDeliveryDate, string Status = "Pending");

record CreateCarOrderRequest(
    [Required] string Make,
    [Required] string Model,
    [Required] string Color
);

record UpdateCarOrderRequest(
    string? Make,
    string? Model,
    string? Color,
    string? Status
);
