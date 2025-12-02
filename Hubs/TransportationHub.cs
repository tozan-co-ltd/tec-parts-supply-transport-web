using Microsoft.AspNetCore.SignalR;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Models;
using tec_parts_supply_transport_web.Repositories;

namespace tec_parts_supply_transport_web.Hubs
{
    public class TransportationHub : Hub
    {
        TransportationRepository transportRepository;

        public TransportationHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            transportRepository = new TransportationRepository(connectionString);
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task SendTransportations()
        {
            try
            {
                // SQL作成
                var sql = transportRepository.CreateSQLToGetTransportation();
                List<PartsModel> listTransports = transportRepository.GetListTransports(sql);
                listTransports = listTransports.Where(x => x.BoxType != null && !x.BoxType.StartsWith("TP")).ToList();
                if (listTransports.Any())
                {
                    var machineStats = listTransports
                        .GroupBy(x => x.MachineNum)
                        .Select(g => new
                        {
                            MachineNum = g.Key,
                            Total = g.Count(),
                            ReadyCount = g.Count(x => x.IsReadyOrder == 1)
                        })
                        .ToDictionary(x => x.MachineNum, x => x);

                    foreach (var item in listTransports)
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
                    await Clients.All.SendAsync("ReceivedTransportations", listTransports);
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
