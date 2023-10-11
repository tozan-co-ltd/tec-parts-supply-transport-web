using tec_empty_box_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_empty_box_supply_transport_web.Commons;

namespace tec_empty_box_supply_transport_web.Repositories
{
    public class TransportRepository
    {
        string connectionString;

        public TransportRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<TransportModel> GetListTransports(string sql)
        {
            // 戻り値
            List<TransportModel> transports = new();

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

                    transports = connection.Query<TransportModel>(sql).ToList();
                }
                return transports;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 運搬取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public string CreateSQLToGetTransport()
        {
            var sql = $@"SELECT 
                            empty_box_supply_request_id AS EmptyBoxSupplyRequestId
                            ,machine_num AS MachineNum
                            ,permanent_abbreviation AS PermanentAbbreviation
                            ,box_type AS BoxType
                            ,box_count AS BoxCount
                            ,request_datetime AS RequestDatetime
                            ,ready_datetime AS ReadyDatetime
                            ,corrected_request_datetime AS CorrectedRequestDatetime
                            ,empty_box_supply_status_id AS EmptyBoxSupplyStatusId
                            FROM t_empty_box_supply_request 
                            WHERE is_deleted = 0 AND empty_box_supply_status_id != 1 AND empty_box_supply_status_id != 4";

            return sql;
        }
    }
}
