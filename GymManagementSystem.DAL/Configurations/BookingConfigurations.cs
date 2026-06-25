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
    public class BookingConfigurations : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            // 1. تجاهل خاصية Id
            builder.Ignore(b => b.Id);

            // 2. إعدادات خصائص التاريخ
            builder.Property(x => x.CreatedAt)
                .HasColumnName("BookingDate")
                .HasDefaultValueSql("GETDATE()");

            #region RelationShips
            // 3. العلاقة مع الـ Session
            builder.HasOne(x => x.Session)
                .WithMany(x => x.SessionMembers)
                .HasForeignKey(x => x.SessionId);

            // 4. العلاقة مع الـ Member
            builder.HasOne(x => x.Member)
                .WithMany(x => x.MemberSessions)
                .HasForeignKey(x => x.MemberId);

            // 5. إعداد المفتاح الأساسي المركب (Composite Key)
            builder.HasKey(x => new { x.MemberId, x.SessionId });
            #endregion
        }
    }
}