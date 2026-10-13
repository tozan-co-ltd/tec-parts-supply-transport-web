using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.IO;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Hubs;
using tec_parts_supply_transport_web.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace tec_parts_supply_transport_web.Controllers
{
    public class PartsController : Controller
    {
        /// <summary>
        /// 準備画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(string type)
        {
            return View();
        }


        /// <summary>
        /// 準備完了登録
        /// </summary>
        /// <param name="dataSupplyId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(string dataSupplyId, bool isRegister)
        {
            try
            {
                string partsSupplyRequestIid = dataSupplyId;
                bool resUpdate = UpdatePartsSupplyRequestForReadyOrCancel(partsSupplyRequestIid, isRegister);
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
        /// 部品準備完了に更新
        /// </summary>
        /// <param name="parts_supply_request_id"></param>
        /// <returns>成功したらtrueを返す</returns>
        public bool UpdatePartsSupplyRequestForReadyOrCancel(string parts_supply_request_id, bool isRegister)
        {
            try
            {
                // 戻り値
                bool isUpdatePartsSupply = false;

                // SQL作成
                var sql = PartsRepository.CreateSQLToUpdatePartsSupplyRequest(parts_supply_request_id, isRegister);

                // DB接続
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    // 戻り値は処理件数
                    var update = connection.Execute(sql, parts_supply_request_id);
                    if (update >= 1)
                    {
                        isUpdatePartsSupply = true;
                    }
                }
                return isUpdatePartsSupply;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 部品完了登録
        /// </summary>
        /// <param name="dataSupplyId"></param>
        /// <param name="machineNum"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Complete(string dataSupplyId, string machineNum, string workType, string dataIsPartsOnlyOder, string dataEmptyBoxId)
        {
            if (string.IsNullOrEmpty(machineNum))
                return Json(new { res = false, message = "機械番号がありません。" });

            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            try
            {
                var sqlGetParts = PartsRepository.CreateSQLToGetPartsIdByMachineNum(dataSupplyId, machineNum, workType, dataIsPartsOnlyOder, dataEmptyBoxId);
                var listParts = PartsRepository.GetListParts(sqlGetParts);

                if (!listParts.Any())
                {
                    return Json(new
                    {
                        res = false,
                        message = "既に完了済みまたは対象データがありません。"
                    });
                }

                // Transaction 1：部品供給AGVの完了状態を更新
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var parts in listParts)
                    {
                        int agv = int.Parse(parts.PartsSupplyAGV);

                        string sqlUpdate = PartsRepository.CreateSQLToUpdateCompletePartsSupplyAGV(parts.PartsSupplyRequestId,workType, agv);

                        int update = connection.Execute(sqlUpdate,transaction: transaction);

                        if (update <= 0)
                        {
                            transaction.Rollback();

                            return Json(new
                            {
                                res = false,
                                message = "部品供給AGVの完了状態を更新できませんでした。"
                            });
                        }
                    }

                    transaction.Commit();
                }

                // Transaction 2：部品在庫を更新
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var parts in listParts)
                    {
                        int agv = int.Parse(parts.PartsSupplyAGV);

                        string updateInventorySql = PartsRepository.CreateSQLToUpdatePartsInventory(workType, agv);

                        if (!string.IsNullOrEmpty(updateInventorySql))
                        {
                            int updateInventory = connection.Execute(updateInventorySql,
                              new
                              {
                                  PartsNum = parts.PartsNum,
                                  Quantity = parts.RequiredQuantity
                              },
                              transaction: transaction);

                            if (updateInventory <= 0)
                            {
                                transaction.Rollback();

                                return Json(new
                                {
                                    res = false,
                                    message = "部品在庫を更新できませんでした。"
                                });
                            }
                        }
                    }

                    transaction.Commit();
                }

                return Json(new { res = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    res = false,
                    errorMessage = ex.Message
                });
            }
        }


        /// <summary>
        /// 欠品登録
        /// </summary>
        /// <param name="dataSupplyId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RegisterOutOfStock(string dataMachineNumber, string password, string workType, string dataIsPartsOnlyOder, string dataSupplyId, string dataEmptyBoxId)
        {
            try
            {
                // パスワードチェック
                var (isValid, error) = await ValidationPassword.ValidatePasswordForRegisterOutOfStock(password);
                if (!isValid)
                    return Json(new { res = false, errorMessage = error });

                // SQL作成
                string sqlUpdate = PartsRepository.CreateSQLToUpdateUpdateOutOfStock(dataMachineNumber, workType, dataIsPartsOnlyOder, dataSupplyId, dataEmptyBoxId);

                bool isUpdatePartsSupply = false;

                // DB接続
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 戻り値は処理件数
                    int update = connection.Execute(sqlUpdate);

                    if (update >= 1)
                        isUpdatePartsSupply = true;
                }

                return Json(new { res = isUpdatePartsSupply, errorMessage = "" });
            }
            catch (ArgumentNullException)
            {
                return Json(new
                {
                    res = false,
                    errorMessage = ErrorHandling.CreateErrorMessage("E2007")
                });
            }
            catch (Exception ex)
            {
                return Json(new { res = false, errorMessage = ex.Message });
            }
        }
    }
}
