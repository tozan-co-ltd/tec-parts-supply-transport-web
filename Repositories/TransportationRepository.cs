using tec_parts_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_parts_supply_transport_web.Commons;
using System.Net;

namespace tec_parts_supply_transport_web.Repositories
{
    public class TransportationRepository
    {
        string connectionString;

        public TransportationRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<PartsModel> GetListTransports(string sql)
        {
            // 戻り値
            List<PartsModel> transports = new();

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

                    transports = connection.Query<PartsModel>(sql).ToList();
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
                                t.parts_supply_request_id AS PartsSupplyRequestId,
                                t.machine_num AS MachineNum,
                                t.parts_num AS PartsNum,
                                t.box_type AS BoxType,
                                t.required_quantity AS RequiredQuantity,
                                t.request_datetime AS RequestDatetime,
                                t.corrected_request_datetime AS CorrectedRequestDatetime,
                                t.is_ready_order AS IsReadyOrder,
                                t.ready_datetime AS ReadyDatetime,
                                t.transportation_start_datetime AS TransportationStartDatetime,
                                t.transportation_end_datetime AS TransportationEndDatetime,
                                t.is_completed AS IsCompleted,
                                t.is_out_of_stock AS IsOutOfStock,
                                t.request_device_name AS RequestDeviceName,
                                t.ready_IPaddress AS ReadyIPaddress,
                                t.transportation_IPaddress AS TransportationIPaddress,
                                t.is_deleted AS IsDeleted,
                                m.count_down_time AS CountDownTime,
                                t.address AS Address
                            FROM t_parts_supply_request AS t
                            LEFT JOIN m_machine_number_basic_information m
                                            ON t.machine_num = m.machine_num
                            WHERE t.is_deleted = 0 AND t.is_out_of_stock = 0;
                        ";
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
