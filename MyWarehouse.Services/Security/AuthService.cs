using AutoMapper;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security.SecurityInterface;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;


namespace MyWarehouse.Services.Security;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService<Users> _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ISupplierUserRepository _supplierUserRepository;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService<Users> passwordService,
        IJwtService jwtService,
        ISupplierUserRepository supplierUserRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _supplierUserRepository = supplierUserRepository;
        _mapper = mapper;
    }

    public async Task<ResponseBase<string>> LoginAsync(LoginDTO loginDTO)
    {
        var response = new ResponseBase<string>();

        try
        {
            var user = await _userRepository.GetByEmailAsync(loginDTO.Email);

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
                var token = _jwtService.GenerateJwtToken(userDto);
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
            bool emailExists = await _userRepository.GetByEmailAsync(registerDTO.Email) != null;

            if (string.IsNullOrEmpty(registerDTO.Password))
            {
                response = ResponseBase<UserDTO>.Fail("La password non può essere vuota", ErrorCode.ValidationError);
            }
            else if (emailExists)
            {
                response = ResponseBase<UserDTO>.Fail("Utente già esistente", ErrorCode.ValidationError);
            }
            else
            {
                var user = _mapper.Map<Users>(registerDTO);
                user.PasswordHash = _passwordService.HashPassword(user, registerDTO.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsDeleted = false;
                user.IdRole = registerDTO.IdRole;

                await _userRepository.AddAsync(user);

                if (user.IdRole == 3 && registerDTO.IdSupplier.HasValue)
                {
                    await _supplierUserRepository.AddSupplierUserAsync(user.Id, registerDTO.IdSupplier.Value);
                }

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
