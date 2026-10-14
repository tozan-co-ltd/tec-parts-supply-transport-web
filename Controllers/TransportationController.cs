using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IO;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Repositories;

namespace tec_parts_supply_transport_web.Controllers
{
    public class TransportationController : Controller
    {
        private readonly ILogger<TransportationController> _logger;

        public TransportationController(ILogger<TransportationController> logger)
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
        /// 運搬開始・運搬終了・部品在庫に更新
        /// </summary>
        /// <param name="dataSupplyId"></param>
        /// <param name="dataPartsNum"></param>
        /// <param name="dataBoxCount"></param>
        /// <param name="statusBtn"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Complete(string dataSupplyId, string dataPartsNum, string dataBoxCount, string statusBtn)
        {
            try
            {
                string partsSupplyRequestId = dataSupplyId;
                var (success, message) = UpdatePartsSupplyRequest(partsSupplyRequestId, dataPartsNum, dataBoxCount, statusBtn);

                if (!success)
                {
                    return Json(new { res = false, message = message });
                }

                return Json(new { res = true });
            }
            catch (Exception ex)
            {
                return Json(new { res = false, message = ex.Message });
            }
        }


        /// <summary>
        /// 運搬開始・運搬終了・部品在庫に更新
        /// </summary>
        public (bool success, string message) UpdatePartsSupplyRequest(
            string empty_box_supply_request_id,
            string partsNum,
            string requiredQuantity,
            string status)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var sql = TransportationRepository.CreateSQLUpdatePartsSupplyRequest(empty_box_supply_request_id, status);

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    int update = connection.Execute(
                        sql,
                        new { empty_box_supply_request_id },
                        transaction: transaction);

                    if (update < 1)
                    {
                        transaction.Rollback();
                        return (false, $"部品 {partsNum} の<br>運搬終了登録ができませんでした。");
                    }

                    // 運搬終了場合、部品在庫更新
                    if (status == "終了")
                    {
                        string updateInventorySql = TransportationRepository.CreateSQLToUpdatePartsInventory();

                        if (string.IsNullOrEmpty(updateInventorySql))
                        {
                            transaction.Rollback();
                            return (false, $"部品 {partsNum} の<br>在庫更新SQLが取得できませんでした。");
                        }

                        int updateInventory = connection.Execute(
                            updateInventorySql,
                            new
                            {
                                PartsNum = partsNum,
                                Quantity = requiredQuantity
                            },
                            transaction: transaction);

                        if (updateInventory <= 0)
                        {
                            transaction.Rollback();
                            return (false, $"部品 {partsNum} の<br>部品在庫更新ができませんでした。");
                        }
                    }

                    transaction.Commit();
                    return (true, string.Empty);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return (false, ex.Message);
                }
            }
        }
    }
}