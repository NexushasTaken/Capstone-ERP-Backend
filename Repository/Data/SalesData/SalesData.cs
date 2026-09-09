using ERP.Repository.Interface.Data.SalesData;

namespace ERP.Repository.Data.Sales
{
    public class SalesData(DatabaseContext _context) : BaseData(_context), ISalesData
    {
    }
}
