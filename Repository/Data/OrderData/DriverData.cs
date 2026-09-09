using ERP.Repository.Configuration.Enum;
using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.OrderData
{
    public class DriverData(DatabaseContext _context) : BaseData(_context), IDriverData
    {
        public IQueryable<DeliveryDriver> FilterQuery(IQueryable<DeliveryDriver> query, string? name, int filter)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(d => d.FirstName.Contains(name.ToLower()) || d.LastName.Contains(name.ToLower()));
            }

            if(Enum.IsDefined(typeof(DriverFilterEnum), filter)){
                var filterEnum = (DriverFilterEnum)filter;

                switch (filterEnum)
                {
                    case DriverFilterEnum.ATOZ:
                        query = query.OrderBy(d => d.FirstName).ThenBy(d => d.LastName);
                        break;
                    case DriverFilterEnum.ZTOA:
                        query = query.OrderByDescending(d => d.FirstName).ThenByDescending(d => d.LastName);
                        break;  
                }
            }
            else
            {
                query = query.OrderByDescending(d => d.Created_At);
            }

            return query;
        }
        public async Task<IEnumerable<DeliveryDriver>> GetDriversWithoutTracking(int page, int pageSize, string? name, int filter, CancellationToken cancellation)
        {
            var driver = BaseQuery<DeliveryDriver>(false).Where(d => d.IsActive == true);

            driver = FilterQuery(driver, name, filter);

            var result = await driver.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellation);

            return result;
        }

        public async Task<int> DriverCount(string? name, int filter)
        {
            var driver = BaseQuery<DeliveryDriver>(false).Where(d => d.IsActive == true);

            driver = FilterQuery(driver, name, filter);

            var result = await driver.CountAsync();

            return result;
        }

        public async Task<DeliveryDriver> GetSingleDriverWithTracking(int id)
        {
            var driver = await BaseQuery<DeliveryDriver>(true).Where(d => d.Id == id && d.IsActive == true).FirstOrDefaultAsync();

            return driver;
        }
    }
}
