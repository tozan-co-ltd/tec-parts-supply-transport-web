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
                string partsSupplyRequestIid = dataSupplyId;
                bool resUpdate = UpdatePartsSupplyRequest(partsSupplyRequestIid, dataPartsNum, dataBoxCount, statusBtn);
                var result = new { res = resUpdate };

                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new { res = ex.Message };

                return Json(result);
            }
        }


        /// <summary>
        /// 運搬開始・運搬終了・部品在庫に更新
        /// </summary>
        /// <param name="empty_box_supply_request_id"></param>
        /// <param name="partsNum"></param>
        /// <param name="requiredQuantity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool UpdatePartsSupplyRequest(
            string empty_box_supply_request_id,
            string partsNum,
            string requiredQuantity,
            string status)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // SQL作成
            var sql = TransportationRepository.CreateSQLUpdatePartsSupplyRequest(empty_box_supply_request_id, status);

            // Transaction 1：運搬開始・運搬終了に更新
            using (var transaction = connection.BeginTransaction())
            {
                int update = connection.Execute(
                    sql,
                    new { empty_box_supply_request_id },
                    transaction: transaction);

                if (update < 1)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
            }

            // 運搬終了の場合
            // Transaction 2：部品在庫を更新
            if (status == "終了")
            {
                using (var transaction = connection.BeginTransaction())
                {
                    string updateInventorySql =TransportationRepository.CreateSQLToUpdatePartsInventory();

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
                        return false;
                    }

                    transaction.Commit();
                }
            }

            return true;
        }
    }
}