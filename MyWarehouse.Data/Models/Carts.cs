using MyWarehouse.Data.Models;

public class Carts : BaseEntity
{
    public int IdUser { get; set; }
    public Users User { get; set; }

    public ICollection<CartItems> Items { get; set; }
}
