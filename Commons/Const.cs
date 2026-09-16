using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tec_parts_supply_transport_web.Commons
{
    public static class Const
    {
        // エリア
        public const int C_AREA_KUBUN = 1; // 1 組立課, 2 プレス課

        public const string C_WORK_LIFT = "Lift"; 

        public const string C_WORK_TAGNOVA= "TagNova";


        /// <summary>
        /// 補充状況のconstant
        /// </summary>
        public const string Const_Restock_Request = "依頼中";
        public const string Const_Restock_Delivery = "搬送中";
        public const string Const_Restock_Success = "補充完了";
        public const string Const_Restock_Fail = "補充失敗";

        public readonly static Dictionary<string, int> Const_Restock_Status = new()
        {
            { Const_Restock_Request, 1 },
            { Const_Restock_Delivery,2 },
            { Const_Restock_Success, 3 },
            { Const_Restock_Fail, 4 }
        };

    }
}
