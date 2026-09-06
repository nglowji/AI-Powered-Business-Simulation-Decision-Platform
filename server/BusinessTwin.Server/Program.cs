using BusinessTwin.Server.Modules.Ai;
using BusinessTwin.Server.Modules.Inventory;
using BusinessTwin.Server.Modules.Sales;
using BusinessTwin.Server.Modules.Purchasing;
using BusinessTwin.Server.Modules.Finance;
using BusinessTwin.Server.Modules.DigitalTwin;
using BusinessTwin.Server.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<BusinessTwinDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BusinessTwin")));
builder.Services.AddSingleton<AiAssistantService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<SalesService>();
builder.Services.AddSingleton<PurchasingService>();
builder.Services.AddSingleton<FinanceService>();
builder.Services.AddSingleton<DigitalTwinService>();

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
app.MapFinanceEndpoints();
app.MapDigitalTwinEndpoints();

app.Run();
