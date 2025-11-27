using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Models;
using tec_parts_supply_transport_web.Repositories;

namespace tec_parts_supply_transport_web.Controllers
{
    public class ZoneController: Controller
    {
        private readonly ILogger<ZoneController> _logger;
        private readonly MMachineRepository mMachineRepository;
        public ZoneController(ILogger<ZoneController> logger)
        {
            _logger = logger;
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            mMachineRepository = new MMachineRepository(connectionString);
        }

        /// <summary>
        /// ゾーン画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            MMachineModel model = new();
            try
            {
                // ゾーンのリスト取得
                model.SearchList = GetZoneList();
                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = "E9999: ";
                ViewData["ErrorMessage"] = errorMessage + ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// ゾーンのりすとを取得
        /// </summary>
        /// <param name="division"></param>
        /// <returns>成功したらtrueを返す</returns>
        public List<MMachineModel> GetZoneList()
        {
            try
            {
                List<MMachineModel> result;

                // SQL作成
                var sql = mMachineRepository.CreateSQLToGetZoneList();

                // DB接続
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    // 稼働状況を取得
                    result = connection.Query<MMachineModel>(sql).ToList();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
