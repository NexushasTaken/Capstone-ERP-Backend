namespace ERP.Repository.ViewModel.Products
{
    public class UnitPostViewModel
    {
        public string? Name { get; set; }
    }

    public class UnitUpdateViewModel : UnitPostViewModel
    {
        public int Id { get; set; }
    }

    public class UnitViewModel : UnitUpdateViewModel
    {
        // Active products counted in this unit; a unit in use can't be deleted
        public int ProductCount { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class UnitPageViewModel
    {
        public IEnumerable<UnitViewModel> Units { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }
}
