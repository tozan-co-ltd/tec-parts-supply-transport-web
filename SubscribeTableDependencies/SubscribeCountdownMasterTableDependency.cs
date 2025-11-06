using tec_parts_supply_transport_web.Hubs;
using tec_parts_supply_transport_web.Models;
using TableDependency.SqlClient;

namespace tec_parts_supply_transport_web.SubscribeTableDependencies
{
    public class SubscribeCountdownMasterTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<MCountdownModel> tableDependency;
        CountdownHub mcountdownHub;

        public SubscribeCountdownMasterTableDependency(CountdownHub mcountdownHub)
        {
            this.mcountdownHub = mcountdownHub;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            try
            {
                tableDependency = new SqlTableDependency<MCountdownModel>(connectionString);
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
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<MCountdownModel> e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                mcountdownHub.SendMCountdown();
            }
        }

        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(MCountdownModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
