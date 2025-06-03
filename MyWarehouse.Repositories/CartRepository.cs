using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class CartRepository : GenericRepository<Carts>, ICartRepository
{
    public CartRepository(WarehouseContext context) : base(context) 
    {
    }

    // restituisce il carrello dell’utente (se esiste)
    public async Task<Carts?> GetCartByUserIdAsync(int userId)
    {
        var cart = await _dbSet
            .FirstOrDefaultAsync(c => c.IdUser == userId);

        return cart;
    }

    // restituisce gli item del carrello con i dati dei prodotti inclusi (per visualizzazione frontend)
    public async Task<List<CartItems>> GetCartItemsWithProductsAsync(int userId)
    {
        var cartItems = await _context.CartItems
            .Where(ci => ci.Cart.IdUser == userId)
            .Include(ci => ci.Product)
            .ToListAsync();

        return cartItems;
    }

    // aggiunge un prodotto al carrello oppure ne aggiorna la quantità
    public async Task AddOrUpdateItemAsync(int userId, int productId, int quantity)
    {
        var cart = await GetCartByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Carts
            {
                IdUser = userId,
                Items = new List<CartItems>()
            };

            await _dbSet.AddAsync(cart);
            await _context.SaveChangesAsync();
        }

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.IdCart == cart.Id && ci.IdProduct == productId);

        if (existingItem != null)
        {
            existingItem.Quantity = quantity;
            _context.CartItems.Update(existingItem);
        }
        else
        {
            var newItem = new CartItems
            {
                IdCart = cart.Id,
                IdProduct = productId,
                Quantity = quantity
            };

            await _context.CartItems.AddAsync(newItem);
        }

        await _context.SaveChangesAsync();
    }

    // rimuove un prodotto dal carrello
    public async Task RemoveItemAsync(int userId, int productId)
    {
        var cart = await GetCartByUserIdAsync(userId);

        if (cart != null)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.IdCart == cart.Id && ci.IdProduct == productId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }

    // svuota completamente il carrello
    public async Task ClearCartAsync(int userId)
    {
        var cart = await GetCartByUserIdAsync(userId);

        if (cart != null)
        {
            var items = _context.CartItems
                .Where(ci => ci.IdCart == cart.Id);

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}