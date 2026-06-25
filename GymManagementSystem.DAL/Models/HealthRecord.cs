using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Models
{
    public class HealthRecord: BaseEntity
    {
        public decimal Weight { get; set; }
        
        public decimal Height { get; set; }
        [Required, MaxLength(5)]
        public string BloodType { get; set; } = default!;

        [ MaxLength(500)]
        public string? Note { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; } = default!;
    }
}
