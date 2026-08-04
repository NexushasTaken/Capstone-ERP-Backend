namespace ERP.Repository.Model
{
    public class BaseModel
    {
        public int Id { get; set; }
        public int? Created_By { get; set; }
        public DateTime? Created_At { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_At { get; set; }
        public int? Deleted_By { get; set; }
        public DateTime? Deleted_At { get; set; }
        public bool IsActive { get; set; }
    }
}
