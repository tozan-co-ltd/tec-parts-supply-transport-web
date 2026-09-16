using Dapper;
using System.Data.SqlClient;
using System.Diagnostics;
using tec_inventory_management_web.Models;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Models;

namespace tec_parts_supply_transport_web.Repositories
{
    public class TRestockingStatusRepository
    {
        string connectionString;

        public TRestockingStatusRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL</param>
        public List<TRestockingStatusModel> ConnectTRestockingStatus(string sql)
        {
            // 戻り値
            List<TRestockingStatusModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSecondSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    strList = connection.Query<TRestockingStatusModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 補充状況取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public string CreateSQLToGetTRestockingStatus(TRestockingStatusModel model)
        {
            // ✅ C#での日付条件の処理
            var dateStartCondition = model.DateSearchStart == null
                ? "1=1"  // 日付条件の処理（フィルターなし）
                : $"FORMAT(replenishment_request_date, 'yyyy-MM-dd HH:mm') >= FORMAT(CAST('{model.DateSearchStart}' AS DATETIME), 'yyyy-MM-dd HH:mm')";

            var dateEndCondition = model.DateSearchEnd == null
                ? "1=1"  // 日付条件の処理（フィルターなし）
                : $"FORMAT(replenishment_request_date, 'yyyy-MM-dd HH:mm') <= FORMAT(CAST('{model.DateSearchEnd}' AS DATETIME), 'yyyy-MM-dd HH:mm')";
            var checkedCondition = model.UserCheck == null
                ? "1=1"
                : $"user_check = {(model.UserCheck == true ? 1 : 0)}";
            // "依頼中"のレコード
            var sql = $@"SELECT 
                            restock_status_id                        AS RestockStatusId
                            ,parts_num                               AS PartsNum
                            ,quantity                                AS Quantity
                            ,restock_status                          AS RestockStatus
                            ,warehouse_information                   AS WarehouseInformation
                            ,replenishment_request_date              AS ReplenishmentRequestDate
                            ,user_check                              AS UserCheck
                        FROM t_restocking_status
                        WHERE restock_status = {model.RestockStatus}  
                     　　　　 AND {dateStartCondition}
                     　　　　 AND {dateEndCondition}
                             AND {checkedCondition}
                             ORDER BY replenishment_request_date DESC";

            return sql;
        }

        /// <summary>
        /// 補充状況更新SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public string CreateSQLToUpdateUserCheck(int restock_status_id, bool user_check)
        {
            var userCheckInt = user_check ? 1 : 0;
            Debug.WriteLine($"userCheckInt: {restock_status_id} {userCheckInt}");
            // "依頼中"のレコード
            var sql = $@"UPDATE t_restocking_status SET user_check = {userCheckInt} 
                        WHERE restock_status_id = {restock_status_id}";

            return sql;
        }
    }
}
