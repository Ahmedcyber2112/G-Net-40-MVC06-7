using GymManagementSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Configurations
{
    public class SessionConfigurations : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            // 1. إعدادات الجدول والقيود (Check Constraints)
            builder.ToTable(T =>
            {
                T.HasCheckConstraint("SessionCapacityConstraint", "Capacity Between 1 and 25");
                T.HasCheckConstraint("SessionEndDateAfterStartDate", "EndDate > StartDate");
            }); // <-- إغلاق البلوك هنا بشكل صحيح

            // 2. إعداد العلاقات (Relationships) خارج بلوك ToTable
            builder.HasOne(x => x.Trainer)
                .WithMany(x => x.TrainerSessions)
                .HasForeignKey(x => x.TrainerId);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.CategoryId);
        }
    }
}
