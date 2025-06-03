namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IAddressRepository : IGenericRepository<Addresses>
{
    IQueryable<Addresses> GetActiveAddressesByUserId(int userId);
    IQueryable<Addresses> GetAllAddressesByUserId(int userId);
    Task<bool> IsAddressOwnedByUserAsync(int addressId, int userId);
}
