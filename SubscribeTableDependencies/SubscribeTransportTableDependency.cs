using tec_empty_box_supply_transport_web.Hubs;
using tec_empty_box_supply_transport_web.Models;
using TableDependency.SqlClient;

namespace tec_empty_box_supply_transport_web.SubscribeTableDependencies
{
    public class SubscribeTransportTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<TransportModel> tableDependency;
        TransportHub transportHub;

        public SubscribeTransportTableDependency(TransportHub transportHub)
        {
            this.transportHub = transportHub;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            try
            {
                tableDependency = new SqlTableDependency<TransportModel>(connectionString);
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
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<TransportModel> e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                transportHub.SendTransports();
            }
        }

        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(TransportModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
