using Microsoft.AspNetCore.SignalR;
using tec_pallet_preparation_transportation_web.Commons;
using tec_pallet_preparation_transportation_web.Hubs;
using tec_pallet_preparation_transportation_web.MiddlewareExtensions;
using tec_pallet_preparation_transportation_web.SubscribeTableDependencies;

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
builder.Services.AddSingleton<TransportationHub>();
builder.Services.AddSingleton<SubscribeTransportationTableDependency>();

var app = builder.Build();

var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();

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
app.MapHub<TransportationHub>("transportationHub");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Top}/{action=Index}/{id?}");
});

app.UseSqlTableDependency<SubscribePreparationTableDependency>(connectionString);
app.UseSqlTableDependency<SubscribeTransportationTableDependency>(connectionString);

app.Run();
