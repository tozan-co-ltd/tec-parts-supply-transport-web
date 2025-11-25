using System.ComponentModel.DataAnnotations.Schema;

namespace tec_parts_supply_transport_web.Models
{
    [Table("m_machine_number_basic_information")]
    public class MMachineModel
    {
        [Column("machine_number_basic_information_id")]
        public int MachineNumberBasicInformationId { get; set; }

        [Column("machine_num")]
        public string MachineNum { get; set; }

        [Column("division")]
        public string Division { get; set; }

        [Column("empty_box_supply_AGV")]
        public string EmptyBoxSupplyAGV { get; set; }

        [Column("parts_supply_AGV")]
        public string PartsSupplyAGV { get; set; }

        [Column("count_down_time")]
        public int CountDownTime { get; set; }

        [Column("zone")]
        public string Zone { get; set; }

        public bool Status { get; set; }
        public DateTime EndTime { get; set; }
    }
}
