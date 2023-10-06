using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
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
                DateTime currentDateTime = DateTime.Now;
                string emptyBoxSupplyRequestIid = dataSupplyId;
                bool resUpdate = UpdateEmptyBoxSupplyRequest(currentDateTime, emptyBoxSupplyRequestIid);
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


        public bool UpdateEmptyBoxSupplyRequest(DateTime ready_datetime, string empty_box_supply_request_id)
        {
            string connectionString = "Data Source=DSP40A\\USA;Initial Catalog=tec-empty-box-supply;User Id=sa;Password=anhminh92";

            string updateQuery = "UPDATE t_empty_box_supply_request SET ready_datetime = @ready_datetime WHERE empty_box_supply_request_id = @empty_box_supply_request_id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        // Parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@ready_datetime", ready_datetime);
                        command.Parameters.AddWithValue("@empty_box_supply_request_id", Convert.ToInt32(empty_box_supply_request_id));

                        // Execute the update query
                        int rowsAffected = command.ExecuteNonQuery();

                        Console.WriteLine($"Rows Affected: {rowsAffected}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
