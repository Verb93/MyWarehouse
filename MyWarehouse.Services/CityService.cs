using AutoMapper;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class CityService : GenericService<Cities, CityDTO>, ICityService
{
    public CityService(ICityRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
