using Microsoft.AspNetCore.SignalR;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Hubs;
using tec_parts_supply_transport_web.MiddlewareExtensions;
using tec_parts_supply_transport_web.Repositories;
using tec_parts_supply_transport_web.SubscribeTableDependencies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(hubOptions => { 
    hubOptions.EnableDetailedErrors = true;
    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10); 
    hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(5);
});

// DI
builder.Services.AddSingleton<PreparationHub>();
builder.Services.AddSingleton<SubscribePreparationTableDependency>();

builder.Services.AddSingleton<SubscribePartsTableDependency>();

builder.Services.AddSingleton<TransportationHub>();
builder.Services.AddSingleton<SubscribeTransportationTableDependency>();

builder.Services.AddSingleton<MachineHub>();
builder.Services.AddSingleton<SubscribeMachineTableDependency>();

builder.Services.AddSingleton<TRestockingStatusHub>();
builder.Services.AddSingleton<SubscribeRestockingStatusTableDependency>();


builder.Services.AddScoped<TRestockingStatusRepository>(provider => 
{
    var connectionString = builder.Configuration.GetConnectionString("tec-inventory-management");
    return new TRestockingStatusRepository(connectionString);
});

var app = builder.Build();

var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
var secondConnetionString = ConnectToSQLServer.GetSecondSQLServerConnectionString();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseWebSockets();

app.MapHub<PreparationHub>("preparationHub");
app.MapHub<PartsHub>("partsHub");
app.MapHub<TransportationHub>("transportationHub");
app.MapHub<MachineHub>("machineHub");
app.MapHub<TRestockingStatusHub>("tRestockingStatusHub");


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Top}/{action=Index}/{id?}");
});

app.UseSqlTableDependency<SubscribePreparationTableDependency>(connectionString);
app.UseSqlTableDependency<SubscribePartsTableDependency>(connectionString);
app.UseSqlTableDependency<SubscribeTransportationTableDependency>(connectionString);
app.UseSqlTableDependency<SubscribeMachineTableDependency>(connectionString);
app.UseSqlTableDependency<SubscribeRestockingStatusTableDependency>(secondConnetionString);


app.Run();
