namespace ERP.Repository.ViewModel.Orders
{
    public class DeliveryDriverViewModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class DeliveryDriverPageViewModel
    {
        public IEnumerable<DeliveryDriverViewModel> DeliveryDrivers { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class DeliveryDriverPostViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class DeliveryDriverPatchViewModel : DeliveryDriverPostViewModel
    {
        public int Id { get; set; }
    }
}
