using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class CityRepository : GenericRepository<Cities>, ICityRepository
{
    public CityRepository(WarehouseContext context) : base(context)
    {
    }

}