using System.ComponentModel.DataAnnotations.Schema;

namespace tec_pallet_preparation_transportation_web.Models
{
    [Table("t_empty_box_supply_request")]
    public class PreparationModel
    {
        [Column("empty_box_supply_request_id")]
        public int EmptyBoxSupplyRequestId { get; set; }

        [Column("machine_num")]
        public string MachineNum { get; set; }

        [Column("permanent_abbreviation")]
        public string PermanentAbbreviation { get; set; }

        [Column("box_type")]
        public string BoxType { get; set; }

        [Column("box_count")]
        public int BoxCount { get; set; }

        [Column("request_datetime")]
        public DateTime RequestDatetime { get; set; }

        [Column("ready_datetime")]
        public DateTime ReadyDatetime { get; set; }

        [Column("ready_IPaddress")]
        public string ReadyIPaddress { get; set; }

        [Column("corrected_request_datetime")]
        public DateTime CorrectedRequestDatetime { get; set; }

        [Column("is_express")]
        public int IsExpress { get; set; }

        [Column("is_deleted")]
        public int IsDeleted { get; set; }

        [Column("empty_box_supply_status_id")]
        public int EmptyBoxSupplyStatusId { get; set; }
    }
}
