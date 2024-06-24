using Microsoft.AspNetCore.SignalR;
using tec_empty_box_supply_transport_web.Commons;
using tec_empty_box_supply_transport_web.Models;
using tec_empty_box_supply_transport_web.Repositories;

namespace tec_empty_box_supply_transport_web.Hubs
{
    public class PreparationHub : Hub
    {
        PreparationRepository supplyRepository;

        public PreparationHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            supplyRepository = new PreparationRepository(connectionString);
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task SendPreparations()
        {
            try
            {
                // SQL作成
                var sql = supplyRepository.CreateSQLToGetPreparations();
                List<PreparationModel> listSupplys = supplyRepository.GetListSupplys(sql);
                if (Clients != null)
                    await Clients.All.SendAsync("ReceivedSupplys", listSupplys);
            }
            catch (Exception)
            {
                // エラーメッセージ作成
                // 「SQLServerでエラーが発生しました。」
                var errorMessage = ErrorHandling.CreateErrorMessage("E4001");
                await Clients.Caller.SendAsync("Error", errorMessage);
            }
        }
    }
}
