namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface ICartRepository : IGenericRepository<Carts>
{
    Task AddOrUpdateItemAsync(int userId, int productId, int quantity);
    Task ClearCartAsync(int userId);
    Task<Carts?> GetCartByUserIdAsync(int userId);
    Task<List<CartItems>> GetCartItemsWithProductsAsync(int userId);
    Task RemoveItemAsync(int userId, int productId);
}
