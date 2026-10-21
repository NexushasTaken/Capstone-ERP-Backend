namespace ERP.Repository.Model.Products
{
    // How a product is counted and sold (pc, box, roll). Quantities stay whole numbers;
    // a pack size goes in the product name, e.g. "wood screw #8 (box of 100)".
    public class Unit : BaseModel
    {
        // Seeded "pc"; the default for new and imported products
        public const int PieceId = 1;

        public string? Name { get; set; }

        public ICollection<Product> Products { get; set; } = [];
    }
}
