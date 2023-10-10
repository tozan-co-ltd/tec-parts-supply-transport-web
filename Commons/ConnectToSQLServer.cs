namespace tec_empty_box_supply_transport_web.Commons
{
    /// <summary>
    /// SQLServer接続に関する関数
    /// </summary>
    public static class ConnectToSQLServer
    {
        /// <summary>
        /// SQLServer接続文字列取得
        /// </summary>
        /// <returns></returns>
        public static string GetSQLServerConnectionString()
        {
            var databaseName = "tec-empty-box-supply";
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            return configuration.GetSection("connectionString").GetValue<string>(databaseName);
        }

        /// <summary>
        /// SQLServer接続文字列取得(集・出荷進捗管理システム)
        /// </summary>
        /// <returns></returns>
        public static string GetSQLServerConnectionStringForTecShippingManagement()
        {
            var databaseName = "tec-shipping-management";
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            return configuration.GetSection("connectionString").GetValue<string>(databaseName);
        }
    }
}
