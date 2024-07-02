using tec_pallet_preparation_transportation_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_pallet_preparation_transportation_web.Commons;

namespace tec_pallet_preparation_transportation_web.Repositories
{
    /// <summary>
    /// エラーメッセージテーブルに関する関数
    /// </summary>
    public static class MErrorMessagesRepository
    {
        /// <summary>
        /// データベースに接続し、SQL実行
        /// </summary>
        /// <param name="sql">SQL</param>
        public static List<MErrorMessagesModel> ConnectMErrorMessages(string sql)
        {
            // 戻り値
            List<MErrorMessagesModel> strList = new();

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

                    strList = connection.Query<MErrorMessagesModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// エラーメッセージ取得SQL作成
        /// </summary>
        /// <remarks>SELECT文 エラーコードが一致するレコード</remarks>
        /// <param name="errorCode">エラーコード</param>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetErrorMessage(string errorCode)
        {
            var sql = $@"
                SELECT
                    error_code              AS ErrorCode
                    ,error_message          AS ErrorMessage
                FROM 
                    m_error_messages
                WHERE
                    error_code              = '{@errorCode}'
            ";

            return sql;
        }
    }
}
