using AutoMapper;
using GymManagementSystem.DAL.Models; // مسار كلاس الـ Plan
using GymManagementSystem.BLL.ViewModels.PlanViewModels; // مسار كلاس الـ PlanViewModel

namespace GymManagementSystem.BLL.Mapping
{
    public class PlanProfile : Profile
    {
        public PlanProfile()
        {
            // السطر ده هو الخريطة اللي بتقوله إزاي يحول بينهم في الاتجاهين
            CreateMap<Plan, PlanViewModel>().ReverseMap();
            CreateMap<Plan, UpdatePlanViewModel>().ReverseMap();
        }
    }
}