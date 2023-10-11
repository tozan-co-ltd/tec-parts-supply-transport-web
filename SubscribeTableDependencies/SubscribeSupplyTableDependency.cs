using tec_empty_box_supply_transport_web.Hubs;
using tec_empty_box_supply_transport_web.Models;
using TableDependency.SqlClient;

namespace tec_empty_box_supply_transport_web.SubscribeTableDependencies
{
    public class SubscribeSupplyTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<SupplyModel> tableDependency;
        SupplyHub supplyHub;

        public SubscribeSupplyTableDependency(SupplyHub supplyHub)
        {
            this.supplyHub = supplyHub;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            tableDependency = new SqlTableDependency<SupplyModel>(connectionString);
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.Start();
        }

        // 変更されたテーブルの依存関係
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<SupplyModel> e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                supplyHub.SendSupplys();
            }
        }

        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(SupplyModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
