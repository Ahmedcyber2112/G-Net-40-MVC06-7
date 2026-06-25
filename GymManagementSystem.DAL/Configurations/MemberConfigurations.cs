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
    public class MemberConfigurations : GymUserConfigurations<Member>,IEntityTypeConfiguration<Member>
    {
        public new void Configure(EntityTypeBuilder<Member> builder)
        {
            // تغيير اسم العمود في الداتا بيز وإعطائه قيمة افتراضية بتاريخ اليوم
            builder.Property(x => x.CreatedAt)
                .HasColumnName("JoinDate")
                .HasDefaultValueSql("GETDATE()");

            // إعداد علاقة 1-to-1 بين العضو والسجل الطبي
            builder.HasOne(x => x.HealthRecord)
                .WithOne(x => x.Member)
                .HasForeignKey<HealthRecord>(x => x.MemberId);

            // تطبيق الإعدادات الموروثة من الكلاس الأساسي (مثل الـ User)
            base.Configure(builder);
        }

    }
}
