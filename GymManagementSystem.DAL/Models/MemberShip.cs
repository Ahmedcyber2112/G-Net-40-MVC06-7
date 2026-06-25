using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Models
{
    public class MemberShip: BaseEntity
    {
        public DateTime EndDate { get; set; }
        public Member Member { get; set; } = null!;

        public int MemberId { get; set; }
        public int PlanId { get; set; }
        public Plan Plan { get; set; }

        [NotMapped]
        public string Status => EndDate > DateTime.UtcNow ? "Expired" : "Active";
        [NotMapped]
        public bool IsActive => EndDate > DateTime.UtcNow;
    }
}
