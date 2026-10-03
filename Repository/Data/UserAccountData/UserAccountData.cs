using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Model.UserAccounts;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.UserAccounts
{
    public class UserAccountData(DatabaseContext _context) : BaseData(_context), IUserAccountData
    {
        public async Task<UserAccount> GetUserByEmailWithoutTracking(string email, CancellationToken cancellation = default)
        {
            var user = await BaseQuery<UserAccount>(false).Include(i => i.UserInformation).Include(r => r.UserRole).FirstOrDefaultAsync(x => x.Email == email, cancellation);

            return user;
        }

        public async Task<UserAccount> GetUserByEmailWithTracking(string email, CancellationToken cancellation = default)
        {
            var user = await BaseQuery<UserAccount>(true).Include(i => i.UserInformation).Include(r => r.UserRole).FirstOrDefaultAsync(x => x.Email == email, cancellation);

            return user;
        }

        public async Task<UserAccount> GetUserByIdWithoutTracking(int userId, CancellationToken cancellation = default)
        {
            var user = await BaseQuery<UserAccount>(false).Include(i => i.UserInformation).Include(r => r.UserRole).FirstOrDefaultAsync(x => x.Id == userId, cancellation);

            return user;
        }
    }
}
