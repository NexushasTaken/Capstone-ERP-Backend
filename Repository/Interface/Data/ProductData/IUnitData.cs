using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;

namespace ERP.Repository.Interface.Data.ProductData
{
    public interface IUnitData : IBaseData
    {
        Task<IEnumerable<UnitViewModel>> GetAllUnitsWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation = default
        );
        Task<int> UnitTotalCount(string? name, CancellationToken cancellation = default);
        Task<Unit?> GetUnitByIdWithTracking(int id);
        Task<bool> UnitExists(int id);
        Task<bool> CheckExistingUnit(string name, int excludeId = 0);
        Task<int> ProductCount(int unitId);
        Task<Dictionary<string, int>> GetUnitIdsByNames(IEnumerable<string> names);
    }
}
