using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Repositories;

namespace tec_parts_supply_transport_web.Controllers
{
    public class TRestockingStatusController: Controller
    {
        private readonly ILogger<TransportationController> _logger;
        private readonly TRestockingStatusRepository tRestockingStatusRepository;

        public TRestockingStatusController(ILogger<TransportationController> logger, TRestockingStatusRepository _tRestockingStatusRepository)
        {
            _logger = logger;
            tRestockingStatusRepository = _tRestockingStatusRepository;
        }

        /// <summary>
        /// 補充状況画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 補充状況の確認
        /// </summary>
        /// <param name="restockStatusId"></param>
        /// <param name="userCheck"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OnCheck(int restockStatusId, bool userCheck)
        {
            try
            {
                bool resUpdate = UpdateTRestockingStatus(restockStatusId, userCheck);
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
        /// 補充状況に更新
        /// </summary>
        /// <param name="restockStatusId"></param>
        /// <param name="userCheck"></param>
        /// <returns>成功したらtrueを返す</returns>
        public bool UpdateTRestockingStatus(int restockStatusId, bool userCheck)
        {
            try
            {
                // SQL作成
                var sql = tRestockingStatusRepository.CreateSQLToUpdateUserCheck(restockStatusId, userCheck);

                // DB接続
                var connectionString = ConnectToSQLServer.GetSecondSQLServerConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    // 戻り値は処理件数
                    var update = connection.Execute(sql);
                    if (update >= 1)
                    {
                        Debug.WriteLine($"update: {userCheck}");
                        //isUpdateEmptyBoxSupply = true;
                    }
                }
                return userCheck;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
