using ERP.Repository.Configuration.Helper;
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

        // Case-insensitive search over first name, last name, email and role. A number also matches the account Id.
        private IQueryable<UserAccount> AccountFilteringQuery(string? name)
        {
            var query = BaseQuery<UserAccount>(false);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = int.TryParse(name.Trim(), out var id);

                query = query.Where(u =>
                    EF.Functions.ILike(u.UserInformation!.FirstName!, pattern) ||
                    EF.Functions.ILike(u.UserInformation!.LastName!, pattern) ||
                    EF.Functions.ILike(u.Email!, pattern) ||
                    EF.Functions.ILike(u.UserRole!.Role!, pattern) ||
                    (hasId && u.Id == id));
            }

            return query;
        }

        // filter: 0 = Id, 1 = Role A-Z, 2 = First name A-Z, 3 = First name Z-A. Ties fall back to Id so paging stays stable.
        private static IQueryable<UserAccount> AccountSortingQuery(IQueryable<UserAccount> query, int filter)
        {
            var sorted = filter switch
            {
                1 => query.OrderBy(u => u.UserRole!.Role),
                2 => query.OrderBy(u => u.UserInformation!.FirstName),
                3 => query.OrderByDescending(u => u.UserInformation!.FirstName),
                _ => query.OrderBy(u => u.Id),
            };

            return sorted.ThenBy(u => u.Id);
        }

        public async Task<IEnumerable<UserAccount>> GetAccountsWithoutTracking(int page, int pageSize, string? name, int filter, CancellationToken cancellation = default)
        {
            var accounts = await AccountSortingQuery(AccountFilteringQuery(name), filter)
                .Include(i => i.UserInformation)
                .Include(r => r.UserRole)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);

            return accounts;
        }

        public async Task<int> AccountTotalCount(string? name, CancellationToken cancellation = default)
        {
            return await AccountFilteringQuery(name).CountAsync(cancellation);
        }

        public async Task<UserRole> GetRoleByName(string roleName, CancellationToken cancellation = default)
        {
            var role = await BaseQuery<UserRole>(false).FirstOrDefaultAsync(r => r.Role!.ToLower() == roleName.ToLower(), cancellation);

            return role;
        }
    }
}
