using ERP.Repository.Interface.Dashboard;
using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        public async Task<IEnumerable<MonthsDataViewModel>> SalesOverView(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
    }
}
