using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security.SecurityInterface;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Interfaces.ServiceInterfaces;
using System.Security.Claims;

namespace MyWarehouse.Services;

public class UserService : GenericService<Users, UserDTO>, IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;
    private readonly IPasswordService<Users> _passwordService;
    private readonly IAuthorizationService _authorizationService;
    public UserService(
        IUserRepository repository, 
        IMapper mapper, 
        IPasswordService<Users> passwordService,
        ISupplierUserRepository supplierUserRepository,
        IJwtService jwtService,
        IAuthorizationService authorizationService
        ) : base(repository, mapper)
    {
        _mapper = mapper;
        _repository = repository;
        _passwordService = passwordService;
        _authorizationService = authorizationService;
    }

    public async Task<ResponseBase<UserDTO>> GetUserByIdAsync(int userId)
    {
        var response = new ResponseBase<UserDTO>();

        try
        {
            var hasAccess = await _authorizationService.CanAccessOwnResourceAsync<Users>(userId, u => u.Id);

            if (!hasAccess)
            {
                response = ResponseBase<UserDTO>.Fail("Non sei autorizzato a visualizzare questo utente.", ErrorCode.Unauthorized);
            }
            else
            {
                var user = await _repository.GetByIdWithRoleAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    response = ResponseBase<UserDTO>.Fail("Utente non trovato.", ErrorCode.NotFound);
                }
                else
                {
                    response = ResponseBase<UserDTO>.Success(_mapper.Map<UserDTO>(user));
                }
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<UserDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public override async Task<IEnumerable<UserDTO>> GetAllAsync()
    {
        var users = await _repository.GetAllWithRoles().ToListAsync();
        return _mapper.Map<IEnumerable<UserDTO>>(users);
    }

    //non elimina completamente l'utante, così da non dover cambiare le altre tabelle associate
    //quindi viene marcato come eliminato
    public async Task<ResponseBase<bool>> DeleteUserAsync(ClaimsPrincipal currentUser, int userId)
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
                // Verifica se l'utente è Admin o sta eliminando sé stesso
                var isAdmin = currentUser.IsInRole("Admin");
                var hasAccess = await _authorizationService.CanAccessOwnResourceAsync<Users>(currentUser, userId, u => u.Id);

                if (!isAdmin && !hasAccess)
                {
                    response = ResponseBase<bool>.Fail("Non sei autorizzato a eliminare questo utente.", ErrorCode.Unauthorized);
                }
                else
                {
                    user.IsDeleted = true;
                    await _repository.UpdateAsync(user);
                    response = ResponseBase<bool>.Success(true);
                }
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<bool>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public async Task<ResponseBase<UserDTO>> UpdateUserAsync(ClaimsPrincipal currentUser, UserDTO userDto)
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
                var hasAccess = await _authorizationService.CanAccessOwnResourceAsync<Users>(currentUser, userDto.Id, u => u.Id);

                if (!hasAccess)
                {
                    response = ResponseBase<UserDTO>.Fail("Non sei autorizzato a modificare questo utente.", ErrorCode.Unauthorized);
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
        }
        catch (Exception ex)
        {
            response = ResponseBase<UserDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public async Task<ResponseBase<bool>> ChangePasswordAsync(ClaimsPrincipal currentUser, int userId, ChangePasswordDTO changePasswordDto)
    {
        var response = new ResponseBase<bool>();

        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<bool>.Fail("Utente non trovato", ErrorCode.NotFound);
            }
            else
            {
                var hasAccess = await _authorizationService.CanAccessOwnResourceAsync<Users>(currentUser, userId, u => u.Id);

                if (!hasAccess)
                {
                    response = ResponseBase<bool>.Fail("Non sei autorizzato a modificare la password di questo utente.", ErrorCode.Unauthorized);
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
        }
        catch (Exception ex)
        {
            response = ResponseBase<bool>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }
}
