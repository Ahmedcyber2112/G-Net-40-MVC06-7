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
    public class GymUserConfigurations<T> : IEntityTypeConfiguration<T> where T :GymUser

    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.Name)
            .HasColumnType("varchar")
             .HasMaxLength(50);

            builder.Property(x => x.Email)
                .HasColumnType("varchar")
                .HasMaxLength(100);

            builder.OwnsOne(x => x.Address, Address =>
            {
                Address.Property(x => x.Street)
                    .HasColumnType("varchar")
                    .HasMaxLength(30);

                Address.Property(x => x.City)
                    .HasColumnType("varchar")
                    .HasMaxLength(30);
            });

            builder.Property(x => x.Phone)
             .HasColumnType("varchar")
             .HasMaxLength(11);

            // التأكد من عدم تكرار الإيميل ورقم الهاتف
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.Phone).IsUnique();

            // إضافة قيود التحقق (Check Constraints) على مستوى الجدول
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("GymUser_EmailCheck", "Email LIKE '_%@_%._%'");
                t.HasCheckConstraint("GymUser_PhoneCheck", "[Phone] LIKE '010%' OR [Phone] LIKE '011%' OR [Phone] LIKE '012%' OR [Phone] LIKE '015%'");
            });
        }
    }
}
