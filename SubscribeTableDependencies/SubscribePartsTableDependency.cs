using tec_parts_supply_transport_web.Hubs;
using tec_parts_supply_transport_web.Models;
using TableDependency.SqlClient;
using tec_parts_supply_transport_web.Commons;
using Microsoft.AspNetCore.SignalR;

namespace tec_parts_supply_transport_web.SubscribeTableDependencies
{
    public class SubscribePartsTableDependency : ISubscribeTableDependency
    {
        private readonly IHubContext<PartsHub> hubContext;
        SqlTableDependency<PartsModel> tableDependency;

        public SubscribePartsTableDependency(IHubContext<PartsHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            try
            {
                tableDependency = new SqlTableDependency<PartsModel>(connectionString);
                tableDependency.OnChanged += TableDependency_OnChanged;
                tableDependency.OnError += TableDependency_OnError;
                tableDependency.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void TableDependency_OnChanged(
    object sender,
    TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<PartsModel> e)
        {
            try
            {
                if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
                {
                    // Lift
                    await hubContext.Clients
                        .Group("Lift")
                        .SendAsync("NotifyPartsChanged", "Lift");

                    // TagNova
                    await hubContext.Clients
                        .Group("TagNova")
                        .SendAsync("NotifyPartsChanged", "TagNova");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(PartsModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
