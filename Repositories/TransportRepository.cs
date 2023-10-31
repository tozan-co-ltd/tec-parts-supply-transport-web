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
                            ,machine_num                AS MachineNum
                            ,permanent_abbreviation     AS PermanentAbbreviation
                            ,box_type                   AS BoxType
                            ,box_count                  AS BoxCount
                            ,request_datetime           AS RequestDatetime
                            ,corrected_request_datetime AS CorrectedRequestDatetime
                            ,ready_datetime             AS ReadyDatetime
                            ,empty_box_supply_status_id AS EmptyBoxSupplyStatusId
                            ,is_express                 AS IsExpress
                        FROM t_empty_box_supply_request 
                        WHERE is_deleted = 0 
                            AND empty_box_supply_status_id != {(int)EnumEmptyBoxSupplyStatus.Requesting} 
                            AND empty_box_supply_status_id != {(int)EnumEmptyBoxSupplyStatus.TransportationEnd}";

            return sql;
        }

        /// <summary>
        /// 運搬開始・運搬終了に更新するSQL作成
        /// </summary>
        /// <param name="empty_box_supply_request_id"></param>
        /// <param name="isDelete"></param>
        /// <param name="status"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLChangeEmptyBoxSupplyStatus(string empty_box_supply_request_id, string status, bool isDelete)
        {
            int statuId = 0;
            var sql = $@"
                    UPDATE
                        t_empty_box_supply_request ";

            if (isDelete)
            {
                if (status == "開始")
                    statuId = (int)EnumEmptyBoxSupplyStatus.Requesting; // 依頼中
                if (status == "終了")
                    statuId = (int)EnumEmptyBoxSupplyStatus.Ready; // 運搬開
            }
            else
            {
                if (status == "開始")
                    statuId = (int)EnumEmptyBoxSupplyStatus.TransportationStart; // 運搬開始
                if (status == "終了")
                    statuId = (int)EnumEmptyBoxSupplyStatus.TransportationEnd; // 運搬終了
            }

            sql += $@"SET
                        empty_box_supply_status_id  = {statuId}";

            if (!isDelete)
            {
                if (status == "開始")
                    sql += $@", transportation_start_datetime = GETDATE()";
                if (status == "終了")
                    sql += $@", transportation_end_datetime = GETDATE(), is_completed = 1 ";
            }

            sql += $@"  WHERE empty_box_supply_request_id = {@empty_box_supply_request_id} ";
            return sql;
        }
    }
}
