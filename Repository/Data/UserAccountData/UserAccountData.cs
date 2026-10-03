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

        public async Task<UserAccount> GetUserByIdWithTracking(int userId, CancellationToken cancellation = default)
        {
            var user = await BaseQuery<UserAccount>(true).Include(i => i.UserInformation).Include(r => r.UserRole).FirstOrDefaultAsync(x => x.Id == userId, cancellation);

            return user;
        }

        public async Task<IEnumerable<UserAccount>> GetAllUsersWithoutTracking(CancellationToken cancellation = default)
        {
            var users = await BaseQuery<UserAccount>(false)
                .Include(i => i.UserInformation)
                .OrderBy(u => u.UserInformation!.FirstName)
                .ThenBy(u => u.UserInformation!.LastName)
                .ToListAsync(cancellation);

            return users;
        }

        public async Task<IEnumerable<UserAccount>> GetAllAccountsWithoutTracking(CancellationToken cancellation = default)
        {
            var accounts = await BaseQuery<UserAccount>(false)
                .Include(i => i.UserInformation)
                .Include(r => r.UserRole)
                .OrderBy(u => u.Id)
                .ToListAsync(cancellation);

            return accounts;
        }

        public async Task<UserRole> GetRoleByName(string roleName, CancellationToken cancellation = default)
        {
            var role = await BaseQuery<UserRole>(false).FirstOrDefaultAsync(r => r.Role!.ToLower() == roleName.ToLower(), cancellation);

            return role;
        }
    }
}
