using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using tec_empty_box_supply_transport_web.Commons;
using tec_empty_box_supply_transport_web.Repositories;

namespace tec_empty_box_supply_transport_web.Controllers
{
    public class SupplyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Complete(string dataSupplyId)
        {
            try
            {
                string emptyBoxSupplyRequestIid = dataSupplyId;
                bool resUpdate = UpdateEmptyBoxSupplyRequest(emptyBoxSupplyRequestIid);
                var result = new { res = resUpdate };

                return Json(result);
            }
            catch (Exception ex)
            {
                // log取得
                var exceptionMessage = ex.Message;
                var result = new { res = exceptionMessage };

                return Json(result);
            }
        }


        /// <summary>
        /// 準備を更新するSQL作成
        /// </summary>
        /// <param>empty_box_supply_request_id</param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLUpdateEmptyBoxSupplyRequest(string empty_box_supply_request_id)
        {
            var sql = $@"
                    UPDATE
                        t_empty_box_supply_request
                    SET
                        ready_datetime  = GETDATE()
                    WHERE empty_box_supply_request_id = {@empty_box_supply_request_id}
            ";
            return sql;
        }


        /// <summary>
        /// 準備を更新する
        /// </summary>
        /// <param>empty_box_supply_request_id</param>
        /// <returns>成功したらtrueを返す</returns>
        public bool UpdateEmptyBoxSupplyRequest(string empty_box_supply_request_id)
        {
            // 戻り値
            bool isUpdateEmptyBoxSupply = false;

            // SQL作成
            var sql = CreateSQLUpdateEmptyBoxSupplyRequest(empty_box_supply_request_id);

            // DB接続
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                // 戻り値は処理件数
                var update = connection.Execute(sql, empty_box_supply_request_id);
                if (update >= 1)
                {
                    isUpdateEmptyBoxSupply = true;
                }
            }
            return isUpdateEmptyBoxSupply;
        }
    }
}
