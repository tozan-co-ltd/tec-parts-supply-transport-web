using tec_parts_supply_transport_web.Hubs;
using tec_parts_supply_transport_web.Models;
using TableDependency.SqlClient;

namespace tec_parts_supply_transport_web.SubscribeTableDependencies
{
    public class SubscribeMachineTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<MMachineModel> inforTableDependency;
        SqlTableDependency<TMachineStatusModel> statusTableDependency;

        MachineHub mMachineHub;

        public SubscribeMachineTableDependency(MachineHub mMachineHub)
        {
            this.mMachineHub = mMachineHub;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            try
            {
                inforTableDependency = new SqlTableDependency<MMachineModel>(connectionString);
                inforTableDependency.OnChanged += TableDependency_OnChanged_MM;
                inforTableDependency.OnError += TableDependency_OnError;
                inforTableDependency.Start();

                statusTableDependency = new SqlTableDependency<TMachineStatusModel>(connectionString);
                statusTableDependency.OnChanged += TableDependency_OnChanged_TS;
                statusTableDependency.OnError += TableDependency_OnError;
                statusTableDependency.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 変更されたテーブルの依存関係
        private void TableDependency_OnChanged_MM(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<MMachineModel>  e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                mMachineHub?.SendMachineStatusList();
            }
        }

        // 変更されたテーブルの依存関係
        private void TableDependency_OnChanged_TS(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<TMachineStatusModel> e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                mMachineHub?.SendMachineStatusList();
            }
        }

        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(MMachineModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
