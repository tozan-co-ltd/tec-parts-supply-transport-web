using tec_parts_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_parts_supply_transport_web.Commons;
using Microsoft.AspNet.SignalR;
using System.Net;

namespace tec_parts_supply_transport_web.Repositories
{
    public class MMachineRepository
    {
        string connectionString;

        public MMachineRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// 稼働状況画面表示
        /// </summary>
        /// <param name="division"></param>
        /// <returns>成功したらtrueを返す</returns>
        public List<MMachineModel> GetMachineStatusList(string sql)
        {
            // 戻り値
            List<MMachineModel> machines = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    machines = connection.Query<MMachineModel>(sql).ToList();
                }
                return machines;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 稼働状況SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public string CreateSQLToGetMMachineList()
        {
            // "依頼中"のレコード
            var sql = $@"SELECT 
                            machine_number.machine_number_basic_information_id AS MachineNumberBasicInformationId
                            ,machine_number.machine_num                        AS MachineNum
                            ,machine_number.division                           AS Division
                            ,machine_number.empty_box_supply_AGV               AS EmptyBoxSupplyAGV
                            ,machine_number.parts_supply_AGV                   AS PartsSupplyAGV
                            ,machine_number.count_down_time                    AS CountDownTime
                            ,machine_number.zone                               AS Zone
                            ,machine_status.status                             AS Status
                            ,machine_status.end_time                           AS EndTime
                        FROM [dbo].[m_machine_number_basic_information] AS machine_number
                        INNER JOIN [dbo].[t_machine_status] AS machine_status ON machine_number.machine_num = machine_status.machine_num
                       ";

            return sql;
        }
    }
}
