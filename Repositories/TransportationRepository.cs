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
                                t.address AS Address,
                                t.supply_location AS SupplyLocation
                            FROM t_parts_supply_request AS t
                            LEFT JOIN m_machine_number_basic_information m
                                            ON t.machine_num = m.machine_num
                            WHERE t.ready_datetime IS NOT NULL AND t.is_ready_order = 1 AND t.is_deleted = 0 AND t.is_completed = 0;
                        ";
            return sql;
        }


        /// <summary>
        /// 運搬開始・運搬終了に更新するSQL作成
        /// </summary>
        /// <param name="parts_supply_request_id"></param>
        /// <param name="isCancelled"></param>
        /// <param name="status"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLUpdatePartsSupplyRequest(string parts_supply_request_id, string status)
        {
            // IPアドレス取得
            string transportationIPaddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "NotFound";

            var sql = @"
            UPDATE
                t_parts_supply_request 
            SET ";

            List<string> sets = new();

            // ▼ IPアドレスは開始時のみ
            if (status == "開始")
                sets.Add($"transportation_IPaddress = '{transportationIPaddress}'");

            // ▼ 運搬開始日時（NULL の時のみ更新）
            if (status == "開始")
            {
                sets.Add(@"
            transportation_start_datetime =
                CASE 
                    WHEN transportation_start_datetime IS NULL THEN GETDATE()
                    ELSE transportation_start_datetime 
                END");
            }

            // ▼ 運搬終了日時（NULL の時のみ更新）＋ 完了フラグ
            if (status == "終了")
            {
                sets.Add(@"
            transportation_end_datetime =
                CASE 
                    WHEN transportation_end_datetime IS NULL THEN GETDATE()
                    ELSE transportation_end_datetime 
                END");
                sets.Add("is_completed = 1");
            }

            sql += string.Join(",", sets);

            sql += $" WHERE parts_supply_request_id = {parts_supply_request_id}";

            return sql;
        }
    }
}
