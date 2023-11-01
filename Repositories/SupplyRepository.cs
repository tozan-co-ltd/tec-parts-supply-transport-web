using tec_empty_box_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_empty_box_supply_transport_web.Commons;
using Microsoft.AspNet.SignalR;
using System.Net;

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
            // "依頼中"のレコード
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
                        WHERE ready_datetime is NULL 
                            AND is_deleted = 0 
                            AND empty_box_supply_status_id = {(int)EnumEmptyBoxSupplyStatus.Requesting} 
                            ORDER BY request_datetime ASC";

            return sql;
        }



        /// <summary>
        /// 準備完了に更新するSQL作成
        /// </summary>
        /// <param name="empty_box_supply_request_id"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToUpdateEmptyBoxSupplyRequest(string empty_box_supply_request_id)
        {
            string readyIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "IPv4アドレスが見つかりません";

            var sql = $@"
                    UPDATE
                        t_empty_box_supply_request
                    SET
                        ready_datetime  = GETDATE()
                        ,empty_box_supply_status_id = {(int)EnumEmptyBoxSupplyStatus.Ready}
                        ,ready_IPaddress = '{@readyIpAddress}'
                    WHERE 
                        empty_box_supply_request_id = {@empty_box_supply_request_id}
            ";
            return sql;
        }
    }
}
