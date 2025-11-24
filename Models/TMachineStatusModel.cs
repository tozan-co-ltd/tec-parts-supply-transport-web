using System.ComponentModel.DataAnnotations.Schema;

namespace tec_parts_supply_transport_web.Models
{
    [Table("t_machine_status")]
    public class TMachineStatusModel
    {
        [Column("machine_number")]
        public string MachineNumber { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("start_time")]
        public string StartTime { get; set; }

        [Column("end_time")]
        public string EndTime { get; set; } 
    }
}