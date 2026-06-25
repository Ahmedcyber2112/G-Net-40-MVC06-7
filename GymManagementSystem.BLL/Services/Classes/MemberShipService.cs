using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.MembershipViewModels;
using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class MemberShipService : IMemberShipService
    {
        private readonly GymDbContext _context;

        public MemberShipService(GymDbContext context)
        {
            _context = context;
        }

        // جلب كل الاشتراكات لعرضها في صفحة الـ Index
        public async Task<IEnumerable<MemberShipViewModel>> GetAllMembershipsAsync()
        {
            return await _context.MemberShips
                .Include(m => m.Member)
                .Include(m => m.Plan)
                .Where(m => m.EndDate >= DateTime.Now) // نجيب الاشتراكات النشطة فقط
                .Select(m => new MemberShipViewModel
                {
                    MemberId = m.MemberId,
                    PlanId = m.PlanId,
                    MemberName = m.Member.Name,
                    PlanName = m.Plan.Name,
                    EndDate = m.EndDate
                }).ToListAsync();
        }

        // جلب المشتركين للـ Dropdown
        public async Task<IEnumerable<MemberSelectListViewModel>> GetMembersForSelectListAsync()
        {
            return await _context.Members
                .Select(m => new MemberSelectListViewModel
                {
                    Id = m.Id,
                    Name = m.Name
                }).ToListAsync();
        }

        // جلب الخطط للـ Dropdown
        public async Task<IEnumerable<PlanSelectListViewModel>> GetPlansForSelectListAsync()
        {
            return await _context.Plans
                .Where(p => p.IsActive)
                .Select(p => new PlanSelectListViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToListAsync();
        }

        // إنشاء اشتراك جديد
        public async Task<bool> CreateMembershipAsync(CreateMemberShipViewModel model)
        {
            var plan = await _context.Plans.FindAsync(model.PlanId);
            if (plan == null) return false;

            // لو اليوزر مدخلش تاريخ البداية، بنعتبره تاريخ النهاردة
            var startDate = model.StartDate ?? DateTime.Now;

            var membership = new MemberShip
            {
                MemberId = model.MemberId,
                PlanId = model.PlanId,
                EndDate = startDate.AddDays(plan.DurationInDays), // بنحسب تاريخ الانتهاء من مدة الخطة
                UpdatedAt = DateTime.Now
            };

            await _context.MemberShips.AddAsync(membership);
            return await _context.SaveChangesAsync() > 0;
        }

        // إلغاء الاشتراك
        public async Task<bool> CancelMembershipAsync(int memberId)
        {
            var membership = await _context.MemberShips
                .FirstOrDefaultAsync(m => m.MemberId == memberId && m.EndDate >= DateTime.Now);

            if (membership == null) return false;

            // للإلغاء، بنخلي تاريخ الانتهاء هو دلوقتي
            membership.EndDate = DateTime.Now;
            membership.UpdatedAt = DateTime.Now;

            _context.MemberShips.Update(membership);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}