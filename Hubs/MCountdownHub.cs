using Microsoft.AspNetCore.SignalR;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Models;
using tec_parts_supply_transport_web.Repositories;
using tec_parts_supply_transport_web.SubscribeTableDependencies;

namespace tec_parts_supply_transport_web.Hubs
{
    public class CountdownHub : Hub
    {
        MCountdownRepository countdownRepository;

        public CountdownHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            countdownRepository = new MCountdownRepository(connectionString);
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task SendMCountdown()
        {
            try
            {
                // SQL作成
                var sql = countdownRepository.CreateSQLToGetMCountdown();
                List<MCountdownModel> listMCountdown = countdownRepository.GetListMCountdown(sql);
                if (Clients != null)
                    await Clients.All.SendAsync("ReceivedMCountdown", listMCountdown.FirstOrDefault().CountdownMinutes);
            }
            catch (Exception ex)
            {
                // エラーメッセージ作成
                // 「SQLServerでエラーが発生しました。」
                var errorMessage = ErrorHandling.CreateErrorMessage("E4001");
                await Clients.Caller.SendAsync("Error", errorMessage);
            }
        }
    }
}
