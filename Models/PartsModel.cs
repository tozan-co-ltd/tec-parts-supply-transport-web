using System.ComponentModel.DataAnnotations.Schema;

namespace tec_parts_supply_transport_web.Models
{
    [Table("t_parts_supply_request")]
    public class PartsModel
    {
        [Column("parts_supply_request_id")]
        public int PartsSupplyRequestId { get; set; }

        [Column("machine_num")]
        public string MachineNum { get; set; }

        [Column("parts_num")]
        public string PartsNum { get; set; }

        [Column("box_type")]
        public string BoxType { get; set; }

        [Column("required_quantity")]
        public int RequiredQuantity { get; set; }

        [Column("request_datetime")]
        public DateTime? RequestDatetime { get; set; }

        [Column("corrected_request_datetime")]
        public DateTime? CorrectedRequestDatetime { get; set; }

        [Column("is_ready_order")]
        public int? IsReadyOrder { get; set; }

        [Column("ready_datetime")]
        public DateTime? ReadyDatetime { get; set; }

        [Column("transportation_start_datetime")]
        public DateTime? TransportationStartDatetime { get; set; }

        [Column("transportation_end_datetime")]
        public DateTime? TransportationEndDatetime { get; set; }

        [Column("is_completed")]
        public int IsCompleted { get; set; }

        [Column("is_out_of_stock")]
        public int IsOutOfStock { get; set; }

        [Column("request_device_name")]
        public string RequestDeviceName { get; set; }

        [Column("ready_IPaddress")]
        public string ReadyIPaddress { get; set; }

        [Column("transportation_IPaddress")]
        public string TransportationIPaddress { get; set; }

        [Column("is_deleted")]
        public int IsDeleted { get; set; }

        public string Address { get; set; }
        [Column("supply_location")]
        public string SupplyLocation { get; set; }

        [NotMapped]
        public string DisplayNumber { get; set; }

        [NotMapped]
        public int ReadyCount { get; set; }

        public int TotalCount { get; set; }

        public int CountDownTime { get; set; }

        public int IsTotalRegister { get; set; }

        public string PartsSupplyAGV { get; set; }

        public int EmptyBoxId { get; set; }    

        public int IsPartsOnlyOder { get; set; }    
    }
}
