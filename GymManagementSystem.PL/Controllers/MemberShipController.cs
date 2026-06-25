using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.MembershipViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagementSystem.PL.Controllers
{
    public class MemberShipController : Controller
    {
        private readonly IMemberShipService _memberShipService;

        public MemberShipController(IMemberShipService memberShipService)
        {
            _memberShipService = memberShipService;
        }

        // عرض صفحة الـ Index
        public async Task<IActionResult> Index()
        {
            var memberships = await _memberShipService.GetAllMembershipsAsync();
            return View(memberships);
        }

        // فتح صفحة الإضافة (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // بنجيب الداتا بتاعت الـ Dropdowns
            var members = await _memberShipService.GetMembersForSelectListAsync();
            var plans = await _memberShipService.GetPlansForSelectListAsync();

            ViewBag.Members = new SelectList(members, "Id", "Name");
            ViewBag.Plans = new SelectList(plans, "Id", "Name");

            return View();
        }

        // استلام البيانات وحفظها (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMemberShipViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isCreated = await _memberShipService.CreateMembershipAsync(model);
                if (isCreated)
                {
                    TempData["SuccessMessage"] = "Membership created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Failed to create membership. Please try again.";
            }

            // لو حصل خطأ بنرجع نحمل الـ Dropdowns تاني
            var members = await _memberShipService.GetMembersForSelectListAsync();
            var plans = await _memberShipService.GetPlansForSelectListAsync();

            ViewBag.Members = new SelectList(members, "Id", "Name", model.MemberId);
            ViewBag.Plans = new SelectList(plans, "Id", "Name", model.PlanId);

            return View(model);
        }

        // زرار الإلغاء
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var isCanceled = await _memberShipService.CancelMembershipAsync(id);
            if (isCanceled)
            {
                TempData["SuccessMessage"] = "Membership has been canceled.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to cancel membership.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}