using BusinessTwin.Server.Modules.Ai;
using BusinessTwin.Server.Modules.Inventory;
using BusinessTwin.Server.Modules.Sales;
using BusinessTwin.Server.Modules.Purchasing;
using BusinessTwin.Server.Modules.Finance;
using BusinessTwin.Server.Modules.DigitalTwin;
using BusinessTwin.Server.Modules.Simulation;
using BusinessTwin.Server.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy => policy
        .WithOrigins("http://127.0.0.1:5173", "http://127.0.0.1:5174", "http://localhost:5173", "http://localhost:5174")
        .AllowAnyHeader()
        .AllowAnyMethod());
});
builder.Services.AddDbContext<BusinessTwinDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BusinessTwin")));
builder.Services.AddSingleton<AiAssistantService>();
builder.Services.AddSingleton<AiAnalyticsService>();
builder.Services.AddSingleton<AlertService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<SalesService>();
builder.Services.AddSingleton<PurchasingService>();
builder.Services.AddSingleton<FinanceService>();
builder.Services.AddSingleton<DigitalTwinService>();
builder.Services.AddSingleton<SimulationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("Client");
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapAiEndpoints();
app.MapAiAnalyticsEndpoints();
app.MapAlertEndpoints();
app.MapInventoryEndpoints();
app.MapSalesEndpoints();
app.MapPurchasingEndpoints();
app.MapFinanceEndpoints();
app.MapDigitalTwinEndpoints();
app.MapSimulationEndpoints();

app.Run();
