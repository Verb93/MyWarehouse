using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security.SecurityInterface;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class UserService : GenericService<Users, UserDTO>, IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;
    private readonly IPasswordService<Users> _passwordService;
    private readonly ISupplierUserRepository _supplierUserRepository;
    private readonly IJwtService _jwtService;
    public UserService(
        IUserRepository repository, 
        IMapper mapper, 
        IPasswordService<Users> passwordService,
        ISupplierUserRepository supplierUserRepository,
        IJwtService jwtService
        ) : base(repository, mapper)
    {
        _mapper = mapper;
        _repository = repository;
        _passwordService = passwordService;
        _supplierUserRepository = supplierUserRepository;
        _jwtService = jwtService;
    }

    public override async Task<IEnumerable<UserDTO>> GetAllAsync()
    {
        var users = await _repository.GetAllWithRoles().ToListAsync();
        return _mapper.Map<IEnumerable<UserDTO>>(users);
    }

    //registra un nuovo utente
    //controllando se l'emai è già registrata
    //hash della password prima di salvarla
    public async Task<ResponseBase<UserDTO>> RegisterUserAsync(RegisterDTO registerDTO)
    {
        var response = new ResponseBase<UserDTO>();
        try 
        {
            bool emailExists = await _repository.GetByEmailAsync(registerDTO.Email) != null;
            if (string.IsNullOrEmpty(registerDTO.Password))
            {
                response = ResponseBase<UserDTO>.Fail("la password non può essere vuota", ErrorCode.ValidationError);
            }
            else if (emailExists)
            {
                response = ResponseBase<UserDTO>.Fail("utente già esistente", ErrorCode.ValidationError);
            }
            else
            {
                var user = _mapper.Map<Users>(registerDTO);
                user.PasswordHash = _passwordService.HashPassword(user, registerDTO.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsDeleted = false;

                var createdUser = await _repository.AddAsync(user);
                response = ResponseBase<UserDTO>.Success(_mapper.Map<UserDTO>(createdUser));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<UserDTO>.Fail($"errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }
        return response;
    }

    //cerca l'utente
    //controlla la password
    public async Task<ResponseBase<bool>> AuthenticateUserAsync(LoginDTO loginDTO, HttpContext httpContext)
    {
        var response = new ResponseBase<bool>();
        try
        {
            var user = await _repository.GetByEmailAsync(loginDTO.Email);

            if (user == null)
            {
                response = ResponseBase<bool>.Fail("Email o password non valide", ErrorCode.ValidationError);
            }
            else if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordService.VerifyPassword(user, user.PasswordHash, loginDTO.Password))
            {
                response = ResponseBase<bool>.Fail("Email o password non valide", ErrorCode.ValidationError);
            }
            else
            {
                httpContext.Session.SetString("UserId", user.Id.ToString());
                httpContext.Session.SetString("UserEmail", user.Email);

                response = ResponseBase<bool>.Success(true);
            }
        }
        catch (Exception ex) 
        {
            response = ResponseBase<bool>.Fail($"errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);

        }
        return response;
    }

    //non elimina completamente l'utante, così da non dover cambiare le altre tabelle associate
    //quindi viene marcato come eliminato
    public async Task<ResponseBase<bool>> DeleteUserAsync(int userId)
    {
        var response = new ResponseBase<bool>();

        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
            {
                response = ResponseBase<bool>.Fail("Utente non trovato", ErrorCode.NotFound);
            }
            else
            {
                user.IsDeleted = true;
                await _repository.UpdateAsync(user);
                response = ResponseBase<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<bool>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public async Task<ResponseBase<UserDTO>> UpdateUserAsync(UserDTO userDto)
    {
        var response = new ResponseBase<UserDTO>();

        try
        {
            var user = await _repository.GetByIdAsync(userDto.Id);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<UserDTO>.Fail("Utente non trovato", ErrorCode.NotFound);
            }
            else
            {
                user.Name = userDto.Name;
                user.Lastname = userDto.Lastname;
                user.BirthDate = userDto.BirthDate;

                if (user.Email != userDto.Email)
                {
                    bool emailExists = await _repository.GetByEmailAsync(userDto.Email) != null;
                    if (emailExists)
                    {
                        response = ResponseBase<UserDTO>.Fail("Email già in uso", ErrorCode.ValidationError);
                    }
                    else
                    {
                        user.Email = userDto.Email;
                    }
                }

                var updatedUser = await _repository.UpdateAsync(user);
                response = ResponseBase<UserDTO>.Success(_mapper.Map<UserDTO>(updatedUser));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<UserDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public async Task<ResponseBase<bool>> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDto)
    {
        var response = new ResponseBase<bool>();

        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<bool>.Fail("Utente non trovato", ErrorCode.NotFound);
            }
            else if (!_passwordService.VerifyPassword(user, user.PasswordHash, changePasswordDto.OldPassword))
            {
                response = ResponseBase<bool>.Fail("La password attuale non è corretta", ErrorCode.ValidationError);
            }
            else if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
            {
                response = ResponseBase<bool>.Fail("La nuova password non può essere vuota", ErrorCode.ValidationError);
            }
            else
            {
                user.PasswordHash = _passwordService.HashPassword(user, changePasswordDto.NewPassword);
                await _repository.UpdateAsync(user);
                response = ResponseBase<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<bool>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }
}
