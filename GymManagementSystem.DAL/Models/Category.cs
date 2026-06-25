using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Models
{
    public class Category: BaseEntity
    {
        public string CategoryName { get; set; } = default!;

        public ICollection<Session> Sessions { get; set; } = null!;
    }
}
