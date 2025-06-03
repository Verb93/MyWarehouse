using MyWarehouse.Data.Models;

public class CartItems : BaseEntity
{
    public int IdCart { get; set; }
    public Carts Cart { get; set; }

    public int IdProduct { get; set; }
    public Products Product { get; set; }

    public int Quantity { get; set; }
}
