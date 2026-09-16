using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using tec_inventory_management_web.Models;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Repositories;

namespace tec_parts_supply_transport_web.Hubs
{
    public class TRestockingStatusHub : Hub
    {
        TRestockingStatusRepository tRestockingStatusRepository;

        public TRestockingStatusHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSecondSQLServerConnectionString();
            tRestockingStatusRepository = new TRestockingStatusRepository(connectionString);
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task SendTRestockingStatusList()
        {
            TRestockingStatusModel param = new TRestockingStatusModel();
            param.RestockStatus = Const.Const_Restock_Status[Const.Const_Restock_Fail];
            param.DateSearchStart = DateTime.Now.AddDays(-2);
            param.DateSearchEnd = DateTime.Now;
          
            try
            {
                // SQL作成
                var sql = tRestockingStatusRepository.CreateSQLToGetTRestockingStatus(param);
                List<TRestockingStatusModel> restockStatusList = tRestockingStatusRepository.ConnectTRestockingStatus(sql);
                if (Clients != null)
                    await Clients.All.SendAsync("ReceivedTRestockingStatusList", restockStatusList);
            }
            catch (Exception)
            {
                // エラーメッセージ作成
                // 「SQLServerでエラーが発生しました。」
                var errorMessage = ErrorHandling.CreateErrorMessage("E4001");
                await Clients.Caller.SendAsync("Error", errorMessage);
            }
        }

        // ✅ Chuyển thành static để gọi từ bên ngoài
        public static List<TRestockingStatusModel> GetRestockingStatusUncheck()
        {
            var connectionString = ConnectToSQLServer.GetSecondSQLServerConnectionString();
            var tRestockingStatusRepository = new TRestockingStatusRepository(connectionString);

            TRestockingStatusModel param = new TRestockingStatusModel();
            param.RestockStatus = Const.Const_Restock_Status[Const.Const_Restock_Fail];
            param.DateSearchStart = null;
            param.DateSearchEnd = DateTime.Now;
            param.UserCheck = false;
            var sql = tRestockingStatusRepository.CreateSQLToGetTRestockingStatus(param);
            return tRestockingStatusRepository.ConnectTRestockingStatus(sql);
        }
    }
}
