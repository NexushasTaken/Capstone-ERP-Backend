using ERP.Repository.Model.Sales;
using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Interface.Data.DashboardData
{
    public interface IDashboardData
    {
        Task<IEnumerable<MonthsDataViewModel>> OverView(DateTime from, DateTime to);
    }
}
