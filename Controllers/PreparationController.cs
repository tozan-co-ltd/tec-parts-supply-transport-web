using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using tec_empty_box_preparation_transportation_web.Commons;
using tec_empty_box_preparation_transportation_web.Repositories;

namespace tec_empty_box_preparation_transportation_web.Controllers
{
    public class PreparationController : Controller
    {
        /// <summary>
        /// 準備画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 準備完了登録
        /// </summary>
        /// <param name="dataSupplyId"></param>
        /// <returns></returns>
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
                var exceptionMessage = ex.Message;
                var result = new { res = exceptionMessage };

                return Json(result);
            }
        }


        /// <summary>
        /// 準備完了に更新
        /// </summary>
        /// <param name="empty_box_supply_request_id"></param>
        /// <returns>成功したらtrueを返す</returns>
        public bool UpdateEmptyBoxSupplyRequest(string empty_box_supply_request_id)
        {
            try
            {
                // 戻り値
                bool isUpdateEmptyBoxSupply = false;

                // SQL作成
                var sql = PreparationRepository.CreateSQLToUpdateEmptyBoxSupplyRequest(empty_box_supply_request_id);

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
            catch (Exception)
            {
                throw;
            }
        }
    }
}
