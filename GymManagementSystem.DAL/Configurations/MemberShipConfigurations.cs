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
    public class MemberShipConfigurations :IEntityTypeConfiguration<MemberShip>
    {
        public void Configure(EntityTypeBuilder<MemberShip> builder)
        {
            // 1. إعداد المفتاح الأساسي
            builder.HasKey(m => m.Id);

            // 2. إعداد عمود تاريخ البداية
            builder.Property(x => x.CreatedAt)
                .HasColumnName("StartDate")
                .HasDefaultValueSql("GETDATE()");

            // 3. إعداد العلاقة مع جدول الخطط (Plan) - مع منع الحذف
            builder.HasOne(m => m.Plan)
                .WithMany(p => p.PlanMembers)
                .HasForeignKey(m => m.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. إعداد العلاقة مع جدول الأعضاء (Member) - مع الحذف التلقائي
            builder.HasOne(M => M.Member)
                .WithMany(m => m.MemberPlans)
                .HasForeignKey(m => m.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
