using tec_parts_supply_transport_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_parts_supply_transport_web.Commons;
using System.Net;

namespace tec_parts_supply_transport_web.Repositories
{
    public class MCountdownRepository
    {
        string connectionString;

        public MCountdownRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<MCountdownModel> GetListMCountdown(string sql)
        {
            // 戻り値
            List<MCountdownModel> mCountdown = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    mCountdown = connection.Query<MCountdownModel>(sql).ToList();
                }
                return mCountdown;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// カウントダウン時間取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public string CreateSQLToGetMCountdown()
        {
            int areaKubun = Const.C_AREA_KUBUN;

            var sql = $@"SELECT 
                    c.machine_num_id    AS MachineNumId, 
                    c.countdown_minutes AS CountdownMinutes
                FROM m_area             AS a
                    JOIN m_machine_num  AS m
                    ON a.area_id        = m.area_id
                    JOIN m_countdown    AS c
                    ON m.machine_num_id = c.machine_num_id
                WHERE a.area_kubun      = {areaKubun}
                  AND a.is_deleted      = 0
                  AND m.is_deleted      = 0
                  AND c.is_deleted      = 0;
                ";  

            return sql;
        }
    }
}
