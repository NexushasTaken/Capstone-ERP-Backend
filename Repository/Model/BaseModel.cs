namespace ERP.Repository.Model
{
    // Lookups and rebuilt tables (statuses, labels, forecast) only need a key.
    public class KeyModel
    {
        public int Id { get; set; }
    }

    // Append-only rows (inventory transactions, damage reports) are never edited or deleted.
    public class CreatedModel : KeyModel
    {
        public int? Created_By { get; set; }
        public DateTime? Created_At { get; set; }
    }

    // Editable entities: update tracking and IsActive soft delete.
    public class BaseModel : CreatedModel
    {
        public int? Updated_By { get; set; }
        public DateTime? Updated_At { get; set; }
        public int? Deleted_By { get; set; }
        public DateTime? Deleted_At { get; set; }
        public bool? IsActive { get; set; }
    }
}
