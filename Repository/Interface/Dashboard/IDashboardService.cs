using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Interface.Dashboard
{
    public interface IDashboardService
    {
        Task<IEnumerable<MonthsDataViewModel>> SalesOverView(DateTime from, DateTime to);
    }
}
