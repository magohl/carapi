using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var carOrders = new List<CarOrder>();

var inventory = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
{
    { "Toyota", ["Camry", "Corolla", "RAV4", "Highlander", "Prius"] },
    { "BMW", ["3 Series", "5 Series", "X3", "X5", "M3"] }
};

var colors = new List<string> { "Black", "White", "Red", "Purple" };

bool IsCarAvailableInInventory(string make, string model, string color)
{
    return inventory.ContainsKey(make)
        && inventory[make].Any(model => model.Equals(model, StringComparison.OrdinalIgnoreCase))
        && colors.Contains(color, StringComparer.OrdinalIgnoreCase);
}

// Root endpoint
app.MapGet("/", () => "Magnus dummy car ordering API...");

// GET /api/cars - Get all car orders
app.MapGet("/api/cars", () => carOrders);

// GET /api/cars/{id} - Get specific car order by ID
app.MapGet("/api/cars/{id:guid}", (Guid id) =>
{
    var order = carOrders.FirstOrDefault(c => c.Id == id);
    return order is not null
        ? Results.Ok(order)
            : Results.NotFound($"Car order with ID {id} not found");
});

// POST /api/cars - Create new car order
app.MapPost("/api/cars", (CreateCarOrderRequest request) =>
{
    if (!IsCarAvailableInInventory(request.Make, request.Model, request.Color))
    {
        return Results.BadRequest($"Car '{request.Make} {request.Model} {request.Color}' is not available");
    }

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
});

// PUT /api/cars/{id} - Update existing car order
app.MapPut("/api/cars/{id:guid}", (Guid id, UpdateCarOrderRequest request) =>
{
    var existingOrder = carOrders.FirstOrDefault(c => c.Id == id);
    if (existingOrder is null)
    {
        return Results.NotFound($"Car order with ID {id} not found");
    }

    var newMake = request.Make ?? existingOrder.Make;
    var newModel = request.Model ?? existingOrder.Model;
    var newColor = request.Color ?? existingOrder.Color;

    if (!IsCarAvailableInInventory(newMake, newModel, newColor))
    {
        return Results.BadRequest($"Car '{newMake} {newModel} {newColor}' is not available");
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
});

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
});

// GET /api/cars/makes - Get available car makes
app.MapGet("/api/cars/makes", () => inventory.Keys.ToArray());

// GET /api/cars/models/{make} - Get models for a specific make
app.MapGet("/api/cars/models/{make}", (string make) =>
{
    if (inventory.TryGetValue(make, out var models))
    {
        return Results.Ok(models.ToArray());
    }

    return Results.NotFound($"No models found for make '{make}'");
});

// GET /api/cars/colors - Get available colors
app.MapGet("/api/cars/colors", () => colors);

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
