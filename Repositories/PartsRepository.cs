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
    public class PartsRepository
    {
        string connectionString;

        public PartsRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public static List<PartsModel> GetListParts(string sql)
        {
            // 戻り値
            List<PartsModel> supplys = new();

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

                    supplys = connection.Query<PartsModel>(sql).ToList();
                }
                return supplys;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 部品準備取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetParts()
        {
            // "依頼中"のレコード
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
                            WHERE t.is_deleted = 0
                              AND t.ready_datetime IS NULL AND t.is_out_of_stock = 0;
                        ";

            return sql;
        }


        /// <summary>
        /// 部品準備取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetPartsIdByMachineNum(string machineNum)
        {
            // "依頼中"のレコード
            var sql = $@"SELECT 
                                t.parts_supply_request_id AS PartsSupplyRequestId
                            FROM t_parts_supply_request AS t
                            WHERE t.is_deleted = 0
                              AND t.machine_num = {machineNum};
                        ";

            return sql;
        }



        /// <summary>
        /// 部品準備に更新するSQL作成
        /// </summary>
        /// <param name="parts_supply_request_id"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToUpdatePartsSupplyRequest(string parts_supply_request_id, bool isRegister)
        {
            // IPアドレス取得
            string readyIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "NotFound";

            var sql = $@"
                    UPDATE t_parts_supply_request
                    SET    is_ready_order   = {(isRegister ? 1 : 0)},
                           ready_IPaddress  = '{readyIpAddress}'
                    WHERE  parts_supply_request_id = {parts_supply_request_id};
                ";
            return sql;
        }


        /// <summary>
        /// 部品準備に更新するSQL作成
        /// </summary>
        /// <param name="parts_supply_request_id"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToUpdateCompletePartsSupplyRequest(int parts_supply_request_id)
        {
            // IPアドレス取得
            string readyIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "NotFound";

            var sql = $@"
                    UPDATE t_parts_supply_request
                    SET is_ready_order   = 1,
                           ready_datetime   = GETDATE(),
                           ready_IPaddress  = '{readyIpAddress}'
                    WHERE  parts_supply_request_id = {parts_supply_request_id};
                ";
            return sql;
        }

        /// <summary>
        /// 部品準備に更新するSQL作成
        /// </summary>
        /// <param name="parts_supply_request_id"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToUpdateUpdateOutOfStock(int parts_supply_request_id)
        {
            // IPアドレス取得
            string readyIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "NotFound";

            var sql = $@"
                    UPDATE t_parts_supply_request
                    SET is_out_of_stock   = 1,
                           ready_IPaddress  = '{readyIpAddress}'
                    WHERE  parts_supply_request_id = {parts_supply_request_id};
                ";
            return sql;
        }
    }
}
