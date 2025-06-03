using AutoMapper;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class RoleService : GenericService<Roles, RoleDTO>, IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;
    public RoleService(IRoleRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _roleRepository = repository;
        _mapper = mapper;
    }

    public async Task<List<RoleDTO>> GetPublicRolesAsync()
    {
        var roles = await _roleRepository.GetPublicRolesAsync();
        return _mapper.Map<List<RoleDTO>>(roles);
    }
}
