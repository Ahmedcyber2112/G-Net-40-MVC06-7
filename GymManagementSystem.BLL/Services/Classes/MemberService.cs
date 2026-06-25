using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels;
using GymManagementSystem.BLL.ViewModels.MemberViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MemberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            var memberRepo = _unitOfWork.GetRepository<Member>();

            var emailExists = await memberRepo.AnyAsync(x => x.Email == model.Email, ct);
            var phoneExists = await memberRepo.AnyAsync(x => x.Phone == model.Phone, ct);

            if (emailExists || phoneExists) return false;

            // استخدام الدالة الداخلية لرفع الصورة وحفظ اسمها
            string photoFileName = UploadPhoto(model.Photo);

            var member = new Member()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                DateOFBirth = model.DateOfBirth,
                Gender = model.Gender,
                Photo = photoFileName, // تخزين اسم الصورة هنا
                Address = new Address()
                {
                    BuildingNumber = model.BuildingNumber,
                    City = model.City,
                    Street = model.Street
                },
                HealthRecord = new HealthRecord()
                {
                    Height = model.HealthRecordViewModel.Height,
                    Weight = model.HealthRecordViewModel.Weight,
                    BloodType = model.HealthRecordViewModel.BloodType,
                    Note = model.HealthRecordViewModel.Note
                }
            };

            await memberRepo.AddAsync(member);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }

        public async Task<IEnumerable<MemberViewModel>> GetALLMembersAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.GetRepository<Member>().GetAllAsync(ct: ct);
            if (!members.Any()) return [];

            var membersViewModel = members.Select(m => new MemberViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Photo = m.Photo,
                Gender = m.Gender.ToString(),
            });

            return membersViewModel;
        }

        public async Task<MemberViewModel?> GetMemberDetailsAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);
            if (member is null) return null;

            var viewmodel = new MemberViewModel()
            {
                Name = member.Name,
                Email = member.Email,
                Phone = member.Phone,
                DateOfBirth = member.DateOFBirth.ToShortDateString(),
                Gender = member.Gender.ToString(),
                Address = $"{member.Address.BuildingNumber}-{member.Address.Street}-{member.Address.City}",
            };

            var activeMembership = await _unitOfWork.GetRepository<MemberShip>().FirstOrDefaultAsync(Mp => Mp.MemberId == memberId
                && Mp.EndDate > DateTime.Now, ct: ct);

            if (activeMembership is not null)
            {
                var activeplan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(activeMembership.PlanId, ct);
                viewmodel.PlanName = activeplan?.Name;
                viewmodel.MembershipStartDate = activeMembership.CreatedAt.ToShortDateString();
                viewmodel.MembershipEndDate = activeMembership.EndDate.ToShortDateString();
            }

            return viewmodel;
        }

        public async Task<HealthRecordViewModel?> GetMemberHealthRecordAsync(int memberId, CancellationToken ct = default)
        {
            var record = await _unitOfWork.GetRepository<HealthRecord>().FirstOrDefaultAsync(x => x.MemberId == memberId, ct: ct);

            if (record is null) return null;

            return new HealthRecordViewModel()
            {
                Height = record.Height,
                Weight = record.Weight,
                BloodType = record.BloodType,
                Note = record.Note
            };
        }

        public async Task<MemberToUpdateViewModel?> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);
            if (member is null) return null;

            return new MemberToUpdateViewModel()
            {
                Name = member.Name,
                Email = member.Email,
                Phone = member.Phone,
                Street = member.Address.Street,
                City = member.Address.City,
                BuildingNumber = member.Address.BuildingNumber,
                Photo = member.Photo
            };
        }

        public async Task<bool> RemoveMemberAsync(int memberId, CancellationToken ct = default)
        {
            var memberRepo = _unitOfWork.GetRepository<Member>();
            var bookingRepo = _unitOfWork.GetRepository<Booking>();

            var member = await memberRepo.GetByIdAsync(memberId, ct);
            if (member is null) return false;

            var hasFutureSessions = await bookingRepo.AnyAsync(b => b.MemberId == memberId && b.Session.StartDate > DateTime.Now);
            if (hasFutureSessions)
            {
                return false;
            }

            string photoFileName = member.Photo; // نحتفظ باسم الصورة قبل الحذف

            await memberRepo.DeleteAsync(member);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            // إذا تم حذف العضو من قاعدة البيانات بنجاح، نحذف صورته من السيرفر
            if (result > 0)
            {
                DeletePhoto(photoFileName);
            }

            return result > 0;
        }

        public async Task<bool> UpdateMemberDetailsAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var memberRepo = _unitOfWork.GetRepository<Member>();

            var member = await memberRepo.GetByIdAsync(id, ct);
            if (member is null) return false;

            if (await memberRepo.AnyAsync(m => m.Email == model.Email && m.Id != id, ct))
                return false;

            if (await memberRepo.AnyAsync(m => m.Phone == model.Phone && m.Id != id, ct))
                return false;

            member.Email = model.Email;
            member.Phone = model.Phone;
            member.Address.City = model.City;
            member.Address.Street = model.Street;
            member.Address.BuildingNumber = model.BuildingNumber;
            member.UpdatedAt = DateTime.Now;

            await memberRepo.UpdateAsync(member);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }

        // ==========================================
        // دوال مساعدة (Private Helpers) للتعامل مع الصور
        // ==========================================

        private string UploadPhoto(IFormFile file)
        {
            if (file is null) return string.Empty;

            // تحديد مسار مجلد الصور
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "members");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // إنشاء اسم فريد للصورة
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(fileStream);

            return fileName;
        }

        private void DeletePhoto(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "members", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}