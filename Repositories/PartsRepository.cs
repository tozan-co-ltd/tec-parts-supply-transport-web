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
                                t.empty_box_id AS EmptyBoxId,
                                t.is_parts_only_order AS IsPartsOnlyOder,
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
                                m.parts_supply_AGV AS PartsSupplyAGV,
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
        public static string CreateSQLToGetPartsIdByMachineNum(
    string machineNum,
    string workType)
        {
            string boxTypeCondition = string.Empty;

            if (workType == Const.C_WORK_LIFT)
                // box_type が TP 以外
                boxTypeCondition = "AND t.box_type NOT LIKE 'TP%'";
            else if (workType == Const.C_WORK_TAGNOVA)
                // box_type が TP
                boxTypeCondition = "AND t.box_type LIKE 'TP%'";

            var sql = $@"
                SELECT 
                    t.parts_supply_request_id AS PartsSupplyRequestId,
                    m.parts_supply_AGV AS PartsSupplyAGV
                FROM t_parts_supply_request AS t
                LEFT JOIN m_machine_number_basic_information AS m
                       ON t.machine_num = m.machine_num
                WHERE t.is_deleted = 0
                  AND t.machine_num = {machineNum}
                  {boxTypeCondition};
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
        public static string CreateSQLToUpdateCompletePartsSupplyAGV(int parts_supply_request_id, string workType, int agv)
        {
            // IPアドレス取得
            string readyIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "NotFound";

            string boxTypeCondition = string.Empty;

            bool canComplete = false;

            if (workType == Const.C_WORK_LIFT)
            {
                // box_type が TP 以外
                boxTypeCondition = "AND box_type NOT LIKE 'TP%'";
                // リフト：AGV = 1 or 3
                canComplete = (agv == 1 || agv == 3);
            }
             
            else if (workType == Const.C_WORK_TAGNOVA)
            {
                // box_type が TP
                boxTypeCondition = "AND box_type LIKE 'TP%'";
                // タグノバ：AGV = 1 or 2
                canComplete = (agv == 1 || agv == 2);
            }
              
            string sql;

            // ready_datetimeのみ登録
            if (!canComplete)
            {
                sql = $@"
                    UPDATE t_parts_supply_request
                    SET ready_datetime = GETDATE(),
                     is_ready_order   = 1,
                     ready_IPaddress  = '{readyIpAddress}'
                    WHERE parts_supply_request_id = {parts_supply_request_id}
                    {boxTypeCondition};
                    ";
            }
            else
            {
                // 完了
                sql = $@"
                        UPDATE t_parts_supply_request
                        SET is_ready_order = 1,
                            ready_datetime = GETDATE(),
                            transportation_start_datetime =
                                CASE 
                                    WHEN transportation_start_datetime IS NULL THEN GETDATE()
                                    ELSE transportation_start_datetime 
                                END,
                            transportation_end_datetime =
                                CASE 
                                    WHEN transportation_end_datetime IS NULL THEN GETDATE()
                                    ELSE transportation_end_datetime 
                                END,
                            is_completed = 1,
                            transportation_IPaddress = '{readyIpAddress}',
                            ready_IPaddress = '{readyIpAddress}'
                        WHERE parts_supply_request_id = {parts_supply_request_id}
                        {boxTypeCondition};
                    ";
                }
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
        /// <param name="dataMachineNumber"></param>
        /// <param name="workType"></param>
        /// <param name="dataIsPartsOnlyOder"></param>
        /// <param name="dataSupplyId"></param>
        /// <param name="dataEmptyBoxId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string CreateSQLToUpdateUpdateOutOfStock(string dataMachineNumber, string workType, string dataIsPartsOnlyOder, string dataSupplyId, string dataEmptyBoxId)
        {
            // IPアドレス取得
            string readyIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "NotFound";

            int supplyId = int.Parse(dataSupplyId); // 部品id
            int isPartsOnlyOder = int.Parse(dataIsPartsOnlyOder); // 部品のみフラグ
            int emptyBoxId = int.Parse(dataEmptyBoxId); // 空箱供給依頼id

            string whereCondition;

            if (isPartsOnlyOder == 1)
                whereCondition = $"parts_supply_request_id = {supplyId}";
            else
            {
                if (workType == Const.C_WORK_LIFT)
                {
                    // LIFT：machine_num = xxx AND box_type NOT LIKE 'TP%'
                    whereCondition = $@"
                    machine_num = {dataMachineNumber}　AND　empty_box_id = {emptyBoxId}
                    AND box_type NOT LIKE 'TP%'";
                }
                else if (workType == Const.C_WORK_TAGNOVA)
                {
                    // TAGNOVA：machine_num = xxx AND box_type LIKE 'TP%'
                    whereCondition = $@"
                    machine_num = {dataMachineNumber}　AND　empty_box_id = {emptyBoxId}
                    AND box_type LIKE 'TP%'";
                }
                else
                {
                    // 念のため（想定外のworkType）
                    throw new ArgumentException($"Invalid workType: {workType}");
                }
            }

            var sql = $@"
                UPDATE t_parts_supply_request
                SET is_out_of_stock  = 1,
                    ready_IPaddress = '{readyIpAddress}'
                WHERE {whereCondition};
            ";

            return sql;
        }
    }
}
