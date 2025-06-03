using MyWarehouse.Data.Models;
using MyWarehouse.Data;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace MyWarehouse.Repositories;

public class SupplierUserRepository : ISupplierUserRepository
{
    private readonly WarehouseContext _context;

    public SupplierUserRepository(WarehouseContext context)
    {
        _context = context;
    }

    // aggiunge l'associazione tra utente e fornitore
    public async Task AddSupplierUserAsync(int userId, int supplierId)
    {
        var entity = new SupplierUsers
        {
            IdUser = userId,
            IdSupplier = supplierId
        };

        _context.SupplierUsers.Add(entity);
        await _context.SaveChangesAsync();
    }

    // elimina l'associazione tra utente e fornitore
    public async Task RemoveSupplierUserAsync(int userId, int supplierId)
    {
        var entity = await _context.SupplierUsers
            .FirstOrDefaultAsync(su => su.IdUser == userId && su.IdSupplier == supplierId);

        if (entity != null)
        {
            _context.SupplierUsers.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveAllSupplierUsersByUserIdAsync(int userId)
    {
        var entities = _context.SupplierUsers
            .Where(su => su.IdUser == userId);

        _context.SupplierUsers.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    // controlla se esiste l'associazione tra utente e fornitore
    public async Task<bool> ExistsAsync(int userId, int supplierId)
    {
        return await _context.SupplierUsers
            .AnyAsync(su => su.IdUser == userId && su.IdSupplier == supplierId);
    }

    // restituisce tutti i fornitori associati a un utente (utile se vuoi gestire supplier multipli)
    public async Task<List<int>> GetSupplierIdsByUserIdAsync(int userId)
    {
        return await _context.SupplierUsers
            .Where(su => su.IdUser == userId)
            .Select(su => su.IdSupplier)
            .ToListAsync();
    }

    // restituisce tutti gli utenti associati a un fornitore (se vuoi vedere chi è legato a un supplier)
    public async Task<List<int>> GetUserIdsBySupplierIdAsync(int supplierId)
    {
        return await _context.SupplierUsers
            .Where(su => su.IdSupplier == supplierId)
            .Select(su => su.IdUser)
            .ToListAsync();
    }
}