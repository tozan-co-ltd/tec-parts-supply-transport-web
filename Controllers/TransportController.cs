using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using tec_empty_box_supply_transport_web.Commons;
using tec_empty_box_supply_transport_web.Models;

namespace tec_empty_box_supply_transport_web.Controllers
{
    public class TransportController : Controller
    {
        private readonly ILogger<TransportController> _logger;

        public TransportController(ILogger<TransportController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 運搬画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 運搬開始・運搬終了に更新
        /// </summary>
        /// <param name="dataSupplyId"></param>
        /// <param name="statusBtn"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Complete(string dataSupplyId, string statusBtn, bool isDelete)
        {
            try
            {
                string emptyBoxSupplyRequestIid = dataSupplyId;
                bool resUpdate = ChangeEmptyBoxSupplyStatus(emptyBoxSupplyRequestIid, statusBtn, isDelete);
                var result = new { res = resUpdate };

                return Json(result);
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                var result = new { res = exceptionMessage };

                return Json(result);
            }
        }


        /// <summary>
        /// 運搬開始・運搬終了に更新
        /// </summary>
        /// <param name="empty_box_supply_request_id"></param>
        /// <returns>成功したらtrueを返す</returns>
        public bool ChangeEmptyBoxSupplyStatus(string empty_box_supply_request_id, string statuId, bool isDelete)
        {
            // 戻り値
            bool isUpdateEmptyBoxSupply = false;

            // SQL作成
            var sql = CreateSQLChangeEmptyBoxSupplyStatus(empty_box_supply_request_id, statuId, isDelete);

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
                    statuId = 1; // 依頼中
                if (status == "終了")
                    statuId = 2; // 運搬開
            }
            else
            {
                if (status == "開始")
                    statuId = 3; // 運搬開始
                if (status == "終了")
                    statuId = 4; // 運搬終了
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