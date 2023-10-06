using Microsoft.AspNetCore.SignalR;
using tec_empty_box_supply_transport_web.Repositories;

namespace tec_empty_box_supply_transport_web.Hubs
{
    public class SupplyHub : Hub
    {
        SupplyRepository supplyRepository;

        public SupplyHub(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            supplyRepository = new SupplyRepository(connectionString);
        }

        public async Task SendSupplys()
        {
            var products = supplyRepository.GetListSupplys();
            await Clients.All.SendAsync("ReceivedSupplys", products);
        }
    }
}
