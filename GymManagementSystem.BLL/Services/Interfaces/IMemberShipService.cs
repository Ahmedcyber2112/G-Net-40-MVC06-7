using GymManagementSystem.BLL.ViewModels.MembershipViewModels; // مسار الـ ViewModels بتاعتك

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface IMemberShipService
    {
        Task<IEnumerable<MemberShipViewModel>> GetAllMembershipsAsync();
        Task<IEnumerable<MemberSelectListViewModel>> GetMembersForSelectListAsync();
        Task<IEnumerable<PlanSelectListViewModel>> GetPlansForSelectListAsync();
        Task<bool> CreateMembershipAsync(CreateMemberShipViewModel model);
        Task<bool> CancelMembershipAsync(int memberId);
    }
}