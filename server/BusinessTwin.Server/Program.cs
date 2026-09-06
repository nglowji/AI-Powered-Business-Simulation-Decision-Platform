using BusinessTwin.Server.Modules.Ai;
using BusinessTwin.Server.Modules.Inventory;
using BusinessTwin.Server.Modules.Sales;
using BusinessTwin.Server.Modules.Purchasing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<AiAssistantService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<SalesService>();
builder.Services.AddSingleton<PurchasingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapAiEndpoints();
app.MapInventoryEndpoints();
app.MapSalesEndpoints();
app.MapPurchasingEndpoints();

app.Run();
