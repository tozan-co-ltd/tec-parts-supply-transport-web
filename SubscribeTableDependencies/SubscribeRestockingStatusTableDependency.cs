using Microsoft.AspNetCore.SignalR;
using TableDependency.SqlClient;
using tec_inventory_management_web.Models;
using tec_parts_supply_transport_web.Hubs;
using tec_parts_supply_transport_web.Models;

namespace tec_parts_supply_transport_web.SubscribeTableDependencies
{
    public class SubscribeRestockingStatusTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<TRestockingStatusModel> tableDependency;

        TRestockingStatusHub tRestockingStatusHub;
        private readonly IHubContext<PartsHub> tPartsHub;

        public SubscribeRestockingStatusTableDependency(TRestockingStatusHub tRestockingStatus, IHubContext<PartsHub> tPartsHub)
        {
            this.tRestockingStatusHub = tRestockingStatus;
            this.tPartsHub = tPartsHub;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            try
            {
                tableDependency = new SqlTableDependency<TRestockingStatusModel>(connectionString);
                tableDependency.OnChanged += TableDependency_OnChanged;
                tableDependency.OnError += TableDependency_OnError;
                tableDependency.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 変更されたテーブルの依存関係
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<TRestockingStatusModel> e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                tRestockingStatusHub?.SendTRestockingStatusList();
                var restockStatusList = TRestockingStatusHub.GetRestockingStatusUncheck();
                if (restockStatusList != null && restockStatusList.Any())
                    tPartsHub.Clients.All.SendAsync("NewErrorAlert", true);
                else tPartsHub.Clients.All.SendAsync("NewErrorAlert", false);
            }
        }

        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(MMachineModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
