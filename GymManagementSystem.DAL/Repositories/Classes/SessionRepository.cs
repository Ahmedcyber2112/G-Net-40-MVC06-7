using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Repositories.Classes
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        private readonly GymDbContext _dbContext;

        public SessionRepository(GymDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Session>> GetAllSessionsWithTrainerAndCategoryAsync(Expression<Func<Session, bool>>? predicate = null, CancellationToken ct = default)
        {
            // ابدأ بالـ Query الأساسية بدون شروط
            IQueryable<Session> query = _dbContext.Sessions
                .Include(s => s.Trainer)    // لا تنسَ الـ Include لجلب البيانات المرتبطة
                .Include(s => s.Category);  // لا تنسَ الـ Include لجلب البيانات المرتبطة

            // أضف الشرط فقط إذا كان موجوداً
            if (predicate is not null)
            {
                query = query.Where(predicate);
            }

            // نفذ الاستعلام
            return await query.ToListAsync(ct);
        }
        

        public Task<int> GetCountOfBookedSlotsAsync(int sessionId, CancellationToken ct = default)
       =>_dbContext.Bookings.AsNoTracking().CountAsync(b => b.SessionId == sessionId, ct);

        public Task<Session?> GetSessionWithTrainerAndCategoryAsync(int SessionId, CancellationToken ct = default)
      => _dbContext.Sessions.AsNoTracking().Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == SessionId, ct);
    }
}
