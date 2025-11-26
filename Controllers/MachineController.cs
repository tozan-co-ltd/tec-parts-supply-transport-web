using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Data.SqlClient;
using tec_parts_supply_transport_web.Commons;
using tec_parts_supply_transport_web.Models;
using tec_parts_supply_transport_web.Repositories;

namespace tec_parts_supply_transport_web.Controllers
{
    public class MachineController : Controller
    {
        private readonly ILogger<MachineController> _logger;
        private readonly MMachineRepository mMachineRepository;
        public MachineController(ILogger<MachineController> logger)
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
        /// 詳しいゾーンデータ画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Status()
        {
            return View();
        }

        /// <summary>
        /// 稼働状況画面表示
        /// </summary>
        /// <param name="division"></param>
        /// <returns>成功したらtrueを返す</returns>
        public List<MMachineModel> GetOperationStatusList()
        {
            try
            {
                List<MMachineModel> result;
                
                // SQL作成
                var sql = mMachineRepository.CreateSQLToGetMMachineList();

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
            catch (Exception)
            {
                throw;
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
