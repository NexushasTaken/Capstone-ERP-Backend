using ERP.Repository.ViewModel.Products;

namespace ERP.Repository.Interface.Products
{
    public interface IUnitService
    {
        Task<UnitPageViewModel> GetUnits(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation
        );
        Task InsertUnit(UnitPostViewModel unit);
        Task UpdateUnit(UnitUpdateViewModel unit);
        Task DeleteUnit(int id);
    }
}
