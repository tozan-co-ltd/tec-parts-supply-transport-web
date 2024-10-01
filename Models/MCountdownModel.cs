using System.ComponentModel.DataAnnotations.Schema;

namespace tec_pallet_supply_transport_web.Models
{
    [Table("m_countdown")]
    public class MCountdownModel
    {
        [Column("countdown_id")]
        public int CountdownId { get; set; }

        [Column("machine_num_id")]
        public int MachineNumId { get; set; }

        [Column("countdown_minutes")]
        public int CountdownMinutes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("updated_by")]
        public string UpdatedBy { get; set; }

        [Column("is_deleted")]
        public int IsDeleted { get; set; }
    }
}
