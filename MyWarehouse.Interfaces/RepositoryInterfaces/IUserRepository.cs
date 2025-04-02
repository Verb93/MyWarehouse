using MyWarehouse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IUserRepository : IGenericRepository<Users>
{
    IQueryable<Users> GetAllWithRoles();
    Task<Users?> GetByEmailAsync(string email);
}
