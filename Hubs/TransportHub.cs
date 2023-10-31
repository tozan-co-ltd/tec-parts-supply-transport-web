using Microsoft.AspNetCore.SignalR;
using tec_empty_box_supply_transport_web.Commons;
using tec_empty_box_supply_transport_web.Models;
using tec_empty_box_supply_transport_web.Repositories;

namespace tec_empty_box_supply_transport_web.Hubs
{
    public class TransportHub : Hub
    {
        TransportRepository transportRepository;

        public TransportHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            transportRepository = new TransportRepository(connectionString);
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task SendTransports()
        {
            try
            {
                // SQL作成
                var sql = transportRepository.CreateSQLToGetTransport();
                List<TransportModel> listTransports = transportRepository.GetListTransports(sql);
                if (Clients != null)
                    await Clients.All.SendAsync("ReceivedTransports", listTransports);
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
