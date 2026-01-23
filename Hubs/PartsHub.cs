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

                if(listSupplys.Any())
{
                    // 空箱のレコード
                    var machineStats = listSupplys
                        .Where(x => x.IsPartsOnlyOder != 1)
                        .GroupBy(x => new { x.MachineNum, x.EmptyBoxId })
                        .Select(g => new
                        {
                            g.Key.MachineNum,
                            g.Key.EmptyBoxId,
                            Total = g.Count(),
                            ReadyCount = g.Count(x => x.IsReadyOrder == 1)
                        })
                        .ToDictionary(
                            x => (x.MachineNum, x.EmptyBoxId),
                            x => x
                        );

                    foreach (var item in listSupplys)
                    {
                        // 部品なら分母が増えない
                        if (item.IsPartsOnlyOder == 1)
                        {
                            item.TotalCount = 1;
                            item.ReadyCount = item.IsReadyOrder == 1 ? 1 : 0;
                            item.DisplayNumber = $"{item.ReadyCount}/1";
                            item.IsTotalRegister = 1; 
                            continue;
                        }

                        // 空箱なら分母が増える
                        var key = (item.MachineNum, item.EmptyBoxId);

                        if (machineStats.TryGetValue(key, out var stat))
                        {
                            item.TotalCount = stat.Total;
                            item.ReadyCount = stat.ReadyCount;
                            item.DisplayNumber = $"{stat.ReadyCount}/{stat.Total}";

                            if (stat.ReadyCount + 1 == stat.Total)
                                item.IsTotalRegister = 1;
                        }
                        else
                        {
                            item.TotalCount = 0;
                            item.ReadyCount = 0;
                            item.DisplayNumber = "0/0";
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
