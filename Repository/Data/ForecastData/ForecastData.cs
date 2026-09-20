using ERP.Repository.Interface.Data.Forecast;
using ERP.Repository.Model.Forecast;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Forecast;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ForecastData
{
    public class ForecastData(DatabaseContext _context) : BaseData(_context), IForecastData
    {
        public async Task<IEnumerable<ForecastViewModel>> Movement()
        {
            var data = await BaseQuery<InventoryTransaction>(false).Include(t => t.Inventory).GroupBy(t => new { t.InventoryId, Day = t.Created_At.HasValue ? t.Created_At.Value.Date : DateTime.MinValue})
                .Select(g => new ForecastViewModel
                {
                    InventoryId = g.Key.InventoryId,
                    Day = g.Key.Day,
                    NetChange = g.Sum(x => x.QuantityChanged),
                    EndDayStock = g.OrderBy(x => x.Created_At).Last().StockLevel
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            return data;
        }

        public async Task<ForecastResult> GetSingleLatestForecast()
        {
            var data = await BaseQuery<ForecastResult>(false).OrderByDescending(x => x.Created_At).FirstOrDefaultAsync();

            return data;
        }

        public async Task<IEnumerable<ForecastResult>> GetThirtyDaysForecast()
        {
            var data = await BaseQuery<ForecastResult>(false)
                .Include(f => f.Inventory)
                .OrderByDescending(f => f.EarliestStockOutDay)
                .Take(30)
                .ToListAsync();

            return data;
        }
    }
}
