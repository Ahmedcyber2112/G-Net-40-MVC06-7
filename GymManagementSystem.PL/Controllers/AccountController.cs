using GymManagementSystem.BLL.ViewModels.AccountViewModels;
using GymManagementSystem.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        // Constructor Injection لجميع الخدمات المطلوبة
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }
        #region Sign Out

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // 1. مسح الكوكيز وجلسة تسجيل الدخول الحالية للمستخدم
            await _signInManager.SignOutAsync();

            // 2. إعادة توجيه المستخدم مباشرة إلى صفحة تسجيل الدخول (Login) بعد الخروج
            return RedirectToAction(nameof(Login));
        }

        #endregion
        #region Login

        // دالة عرض صفحة تسجيل الدخول (GET)
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // 1. التحقق من صحة الـ Validation بالـ View Model (مثل الـ Required)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. البحث عن المستخدم في قاعدة البيانات بواسطة البريد الإلكتروني
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is not null)
            {
                // 3. التحقق من صحة كلمة المرور وعمل تسجيل الدخول
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RemeberMe, false);

                if (result.Succeeded)
                {
                    // 4. التوجيه إلى الصفحة الرئيسية للموقع عند نجاح تسجيل الدخول
                    return RedirectToAction("Index", "Home");
                }
            }

            // 5. إضافة خطأ مخصص في حال فشل تسجيل الدخول أو عدم مطابقة البيانات
            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

            return View(model);
        }
        #endregion
        #region AccessDenied

        // دالة توجيه المستخدم لصفحة "غير مسموح بالدخول" عند محاولة فتح صفحة لا يملك الـ Role الخاص بها
        public IActionResult AccessDenied() => View();

        #endregion
    }
}
