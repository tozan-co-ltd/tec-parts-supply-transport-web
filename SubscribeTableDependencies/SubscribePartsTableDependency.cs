using tec_parts_supply_transport_web.Hubs;
using tec_parts_supply_transport_web.Models;
using TableDependency.SqlClient;
using tec_parts_supply_transport_web.Commons;

namespace tec_parts_supply_transport_web.SubscribeTableDependencies
{
    public class SubscribePartsTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<PartsModel> tableDependency;
        PartsHub partsHub;

        public SubscribePartsTableDependency(PartsHub partsHub)
        {
            this.partsHub = partsHub;
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

        // 変更されたテーブルの依存関係
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<PartsModel> e)
        {
            try
            {
                // データを更新される時HUBのメソッドを呼びます
                if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
                {
                    partsHub.SendParts();
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
