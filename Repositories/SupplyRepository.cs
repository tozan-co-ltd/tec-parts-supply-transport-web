using tec_empty_box_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace tec_empty_box_supply_transport_web.Repositories
{
    public class SupplyRepository
    {
        string connectionString;

        public SupplyRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<SupplyModel> GetListSupplys()
        {
            List<SupplyModel> supplys = new List<SupplyModel>();
            SupplyModel supply;
            try
            {
                var data = GetSupplysFromDb();

                foreach (DataRow row in data.Rows)
                {
                    supply = new SupplyModel
                    {
                        EmptyBoxSupplyRequestId = Convert.ToInt32(row["empty_box_supply_request_id"]),
                        MachineNum = row["machine_num"].ToString(),
                        PermanentAbbreviation = row["permanent_abbreviation"].ToString(),
                        BoxType = Convert.ToInt32(row["box_type"]),
                        BoxCount = Convert.ToInt32(row["box_count"]),
                        CorrectedRequestDatetime = Convert.ToDateTime(row["corrected_request_datetime"])
                    };
                    supplys.Add(supply);
                }
            }
            catch(Exception ex)
            {

            }
            return supplys;
        }

        private DataTable GetSupplysFromDb()
        {
            var query = "SELECT *  FROM t_empty_box_supply_request WHERE ready_datetime is NULL";
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }

                    return dataTable;
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
