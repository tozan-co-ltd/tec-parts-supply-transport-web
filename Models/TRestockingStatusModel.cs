using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tec_inventory_management_web.Models
{
    [Table("t_restocking_status")]
    public class TRestockingStatusModel
    {
        /// <summary>
        /// ID
        /// </summary>
        [Display(Name = "ID")]
        [Column("restock_status_id")]
        public string RestockStatusId { set; get; }

        /// <summary>
        /// 検索年月日（開始）
        /// </summary>
        public DateTime? DateSearchStart { set; get; } = null;

        /// <summary>
        /// 検索年月日 (終了)
        /// </summary>
        public DateTime? DateSearchEnd { set; get; } = null;

        /// <summary>
        /// 品番
        /// </summary>
        [Display(Name = "品番")]
        [Column("parts_num")]
        public string PartsNum { set; get; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        [Column("quantity")]
        public int Quantity { set; get; }

        /// <summary>
        /// 補充状況
        /// 1：依頼中
        /// 2：搬送中
        /// 3：補充完了
        /// 4：補充失敗
        /// </summary>
        [Display(Name = "補充状況")]
        [Column("restock_status")]
        public int? RestockStatus { set; get; }

        /// <summary>
        /// 補充元
        /// 0：自動倉庫
        /// 1：通常倉庫
        /// </summary>
        [Display(Name = "補充元")]
        [Column("warehouse_information")]
        public int WarehouseInformation { set; get; }

        /// <summary>
        /// 補充依頼日時
        /// </summary>
        [Display(Name = "補充依頼日時")]
        [Column("replenishment_request_date")]
        public DateTime ReplenishmentRequestDate { set; get; }

        /// <summary>
        /// 補充完了日時
        /// </summary>
        [Display(Name = "補充完了日時")]
        [Column("replenishment_complete_date")]
        public DateTime ReplenishmentCompleteDate { set; get; }

        /// <summary>
        /// 補充失敗チェック
        /// </summary>
        [Display(Name = "補充失敗チェック")]
        [Column("user_check")]
        public bool? UserCheck { set; get; }
    }
}
