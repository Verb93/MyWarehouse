using AutoMapper;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security.SecurityInterface;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Interfaces.ServiceInterfaces;
using MyWarehouse.Common.Constants;
using MyWarehouse.Common.Requests;
using Microsoft.AspNetCore.Authorization;


namespace MyWarehouse.Services.Security;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService<Users> _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ISupplierUserRepository _supplierUserRepository;
    private readonly IRoleService _roleService;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService<Users> passwordService,
        IJwtService jwtService,
        ISupplierUserRepository supplierUserRepository,
        IRoleService roleService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _supplierUserRepository = supplierUserRepository;
        _roleService = roleService;
        _mapper = mapper;
    }

    public async Task<ResponseBase<string>> LoginAsync(LoginDTO loginDTO)
    {
        var response = new ResponseBase<string>();

        try
        {
            var user = await _userRepository.GetAllWithRoles().FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

            if (user == null)
            {
                response = ResponseBase<string>.Fail("Email o password non valide", ErrorCode.ValidationError);
            }
            else if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordService.VerifyPassword(user, user.PasswordHash, loginDTO.Password))
            {
                response = ResponseBase<string>.Fail("Email o password non valide", ErrorCode.ValidationError);
            }
            else
            {
                var userDto = _mapper.Map<UserDTO>(user);
                var roleNames = user.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList();
                var token = _jwtService.GenerateJwtToken(userDto, roleNames);

                response = ResponseBase<string>.Success(token);
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<string>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public async Task<ResponseBase<UserDTO>> RegisterAsync(RegisterDTO registerDTO)
    {
        var response = new ResponseBase<UserDTO>();

        try
        {
            // 1. Verifica se esiste un utente con la stessa email
            var existingUser = await _userRepository.GetByEmailAsync(registerDTO.Email);
            if (existingUser != null)
            {
                response = ResponseBase<UserDTO>.Fail("Utente già esistente", ErrorCode.ValidationError);
            }
            else if (string.IsNullOrWhiteSpace(registerDTO.Password))
            {
                response = ResponseBase<UserDTO>.Fail("La password non può essere vuota", ErrorCode.ValidationError);
            }
            else
            {
                // 2. Crea nuovo utente
                var user = _mapper.Map<Users>(registerDTO);
                user.PasswordHash = _passwordService.HashPassword(user, registerDTO.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsDeleted = false;

                await _userRepository.AddAsync(user);

                // 3. Assegna ruolo client
                var publicRoles = await _roleService.GetPublicRolesAsync();
                var client = publicRoles.FirstOrDefault(r => r.Name.Equals(RoleNames.Client, StringComparison.OrdinalIgnoreCase));
                if (client != null)
                {
                    await _userRepository.AddUserRolesAsync(user.Id, new List<int> { client.Id });
                }

                // 4. Ritorna l'utente creato
                var createdUser = await _userRepository.GetAllWithRoles()
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                response = ResponseBase<UserDTO>.Success(_mapper.Map<UserDTO>(createdUser));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<UserDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }
}
