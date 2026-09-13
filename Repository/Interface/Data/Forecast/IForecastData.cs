using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.Data.Forecast
{
    public interface IForecastData : IBaseData
    {
        Task<IEnumerable<ForecastViewModel>> Movement();
    }
}
