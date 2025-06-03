using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class AddressRepository : GenericRepository<Addresses>, IAddressRepository
{
    public AddressRepository(WarehouseContext context) : base(context) 
    {
    }

    public async Task<bool> IsAddressOwnedByUserAsync(int addressId, int userId)
    {
        return await _dbSet.AnyAsync(a => a.Id == addressId && a.IdUser == userId);
    }

    //Per la sezione "I miei indirizzi": anche disattivati
    public IQueryable<Addresses> GetAllAddressesByUserId(int userId)
    {
        return _dbSet
            .Where(a => a.IdUser == userId)
            .Include(a => a.City);
    }

    //Per il checkout: solo indirizzi attivi
    public IQueryable<Addresses> GetActiveAddressesByUserId(int userId)
    {
        return _dbSet
            .Where(a => a.IdUser == userId && !a.IsDeleted)
            .Include(a => a.City);
    }

}