namespace ERP.Repository.Model.Inventories
{
    public class DeliveryDriver : BaseModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public ICollection<Orders> Orders { get; set; } = [];
    }
}
