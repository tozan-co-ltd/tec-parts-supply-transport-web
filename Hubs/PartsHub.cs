using Microsoft.AspNetCore.SignalR;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Models;
using tec_parts_supply_transport_web.Repositories;

namespace tec_parts_supply_transport_web.Hubs
{
    public class PartsHub : Hub
    {
        public PartsHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task SendParts(string workType)
        {
            try
            {
                // SQL作成
                var sql = PartsRepository.CreateSQLToGetParts();
                List<PartsModel> listSupplys = PartsRepository.GetListParts(sql);

                // ---- 分岐処理 ----
                if (workType == Const.C_WORK_LIFT)
                    listSupplys = listSupplys.Where(x => x.BoxType != null && !x.BoxType.StartsWith("TP")).ToList();
                else if (workType == Const.C_WORK_TAGNOVA)
                    listSupplys = listSupplys.Where(x => x.BoxType != null && x.BoxType.StartsWith("TP")).ToList();

                if (listSupplys.Any())
                {
                    var machineStats = listSupplys
                        .GroupBy(x => x.MachineNum)
                        .Select(g => new
                        {
                            MachineNum = g.Key,
                            Total = g.Count(),
                            ReadyCount = g.Count(x => x.IsReadyOrder == 1)
                        })
                        .ToDictionary(x => x.MachineNum, x => x);

                    foreach (var item in listSupplys)
                    {
                        if (machineStats.TryGetValue(item.MachineNum, out var stat))
                        {
                            item.DisplayNumber = $"{stat.ReadyCount}/{stat.Total}";
                            item.ReadyCount = stat.ReadyCount;
                            item.TotalCount = stat.Total;
                            if (stat.ReadyCount + 1 == stat.Total)
                                item.IsTotalRegister = 1;
                        }
                        else
                        {
                            item.DisplayNumber = "0/0";
                            item.ReadyCount = 0;
                            item.TotalCount = 0;
                        }
                    }
                }

                if (Clients != null)
                    await Clients.Group(workType).SendAsync("ReceivedSupplys", listSupplys);
            }
            catch (Exception)
            {
                // エラーメッセージ作成
                // 「SQLServerでエラーが発生しました。」
                var errorMessage = ErrorHandling.CreateErrorMessage("E4001");
                await Clients.Caller.SendAsync("Error", errorMessage);
            }
        }

        public async Task JoinGroup(string workType)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, workType);
        }
    }
}
