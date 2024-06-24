using tec_empty_box_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_empty_box_supply_transport_web.Commons;
using System.Net;

namespace tec_empty_box_supply_transport_web.Repositories
{
    public class TransportationRepository
    {
        string connectionString;

        public TransportationRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<TransportationModel> GetListTransports(string sql)
        {
            // 戻り値
            List<TransportationModel> transports = new();

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

                    transports = connection.Query<TransportationModel>(sql).ToList();
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
        public string CreateSQLToGetTransportation()
        {
            // "準備完了"、"運搬開始"のレコード
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
                        WHERE transportation_end_datetime is NULL 
                            AND is_deleted = 0 
                            AND empty_box_supply_status_id != {(int)EnumEmptyBoxSupplyStatus.Requesting} 
                            AND empty_box_supply_status_id != {(int)EnumEmptyBoxSupplyStatus.TransportationEnd}
                            ORDER BY request_datetime ASC";

            return sql;
        }


        /// <summary>
        /// 運搬開始・運搬終了に更新するSQL作成
        /// </summary>
        /// <param name="empty_box_supply_request_id"></param>
        /// <param name="isCancelled"></param>
        /// <param name="status"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLChangeEmptyBoxSupplyStatus(string empty_box_supply_request_id, string status, bool isCancelled)
        {
            // IPアドレス取得
            string transportationIPaddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "NotFound";

            int statuId = 0;
            var sql = $@"
                    UPDATE
                        t_empty_box_supply_request ";

            if (isCancelled)
            {
                // 開始ボタンの隣の取消ボタンの場合は、依頼中に変更
                if (status == "開始")
                    statuId = (int)EnumEmptyBoxSupplyStatus.Requesting;
                // 終了ボタンの隣の取消ボタンの場合は、準備完了に変更
                if (status == "終了")
                    statuId = (int)EnumEmptyBoxSupplyStatus.Ready;
            }
            else
            {
                // 開始ボタンの場合は、運搬開始に変更
                if (status == "開始")
                    statuId = (int)EnumEmptyBoxSupplyStatus.TransportationStart;
                // 終了ボタンの場合は、運搬終了に変更
                if (status == "終了")
                    statuId = (int)EnumEmptyBoxSupplyStatus.TransportationEnd;
            }

            sql += $@"SET
                        empty_box_supply_status_id  = {statuId}
                        , transportation_IPaddress  = '{@transportationIPaddress}'";

            if (isCancelled)
            {
                // 開始ボタンの隣の取消ボタンの場合は、準備完了日時をNULL
                if (status == "開始")
                    sql += $@", ready_datetime = NULL";
                // 終了ボタンの隣の取消ボタンの場合は、運搬開始日時をNULL
                if (status == "終了")
                    sql += $@", transportation_start_datetime = NULL";
            }
            else
            {
                // 開始ボタンの場合
                if (status == "開始")
                    sql += $@", transportation_start_datetime = GETDATE()";
                // 終了ボタンの場合は、完了フラグ=1
                if (status == "終了")
                    sql += $@", transportation_end_datetime = GETDATE(), is_completed = 1";
            }

            sql += $@" WHERE empty_box_supply_request_id = {@empty_box_supply_request_id} ";
            return sql;
        }
    }
}
