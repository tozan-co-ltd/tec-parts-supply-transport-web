using tec_empty_box_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_empty_box_supply_transport_web.Commons;

namespace tec_empty_box_supply_transport_web.Repositories
{
    public class SupplyRepository
    {
        string connectionString;

        public SupplyRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<SupplyModel> GetListSupplys(string sql)
        {
            // 戻り値
            List<SupplyModel> supplys = new();

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

                    supplys = connection.Query<SupplyModel>(sql).ToList();
                }
                return supplys;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 準備取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public string CreateSQLToGetSupplys()
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
                            ,is_express AS IsExpress
                            FROM t_empty_box_supply_request 
                            WHERE ready_datetime is NULL AND is_deleted = 0 ORDER BY request_datetime ASC";

            return sql;
        }
    }
}
