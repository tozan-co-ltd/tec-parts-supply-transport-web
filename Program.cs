using Microsoft.AspNetCore.SignalR;
using tec_empty_box_supply_transport_web.Commons;
using tec_empty_box_supply_transport_web.Hubs;
using tec_empty_box_supply_transport_web.MiddlewareExtensions;
using tec_empty_box_supply_transport_web.SubscribeTableDependencies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// DI
builder.Services.AddSingleton<SupplyHub>();
builder.Services.AddSingleton<SubscribeSupplyTableDependency>();

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

app.MapHub<SupplyHub>("/supplyHub");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Top}/{action=Index}/{id?}");
});

app.UseSqlTableDependency<SubscribeSupplyTableDependency>(connectionString);

app.Run();
