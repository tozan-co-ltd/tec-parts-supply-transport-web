using Microsoft.AspNetCore.SignalR;
using tec_empty_box_supply_transport_web.Commons;
using tec_empty_box_supply_transport_web.Models;
using tec_empty_box_supply_transport_web.Repositories;

namespace tec_empty_box_supply_transport_web.Hubs
{
    public class SupplyHub : Hub
    {
        SupplyRepository supplyRepository;

        public SupplyHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            supplyRepository = new SupplyRepository(connectionString);
        }

        public async Task SendSupplys()
        {
            try
            {
                // SQL作成
                var sql = supplyRepository.CreateSQLToGetSupplys();
                List<SupplyModel> listSupplys = supplyRepository.GetListSupplys(sql);
                await Clients.All.SendAsync("ReceivedSupplys", listSupplys);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
