using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.Constants;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Requests;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security.SecurityInterface;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class UserService : GenericService<Users, UserDTO>, IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;
    private readonly IPasswordService<Users> _passwordService;
    private readonly ISupplierUserRepository _supplierUserRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRoleRepository _roleRepository;

    public UserService(
        IUserRepository repository,
        IMapper mapper,
        IPasswordService<Users> passwordService,
        ISupplierUserRepository supplierUserRepository,
        IAuthorizationService authorizationService,
        IRoleRepository roleRepository) : base(repository, mapper)
    {
        _mapper = mapper;
        _repository = repository;
        _passwordService = passwordService;
        _supplierUserRepository = supplierUserRepository;
        _authorizationService = authorizationService;
        _roleRepository = roleRepository;
    }

    public async Task<ResponseBase<UserDTO>> GetUserByIdAsync(int userId)
    {
        var response = new ResponseBase<UserDTO>();

        try
        {
            var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanViewUser");
            var currentUserId = _authorizationService.GetCurrentUserId();
            var (hasGlobalPermission, _) = await _authorizationService.HasPermissionAsync(userId, "CanViewAllUsers");

            if (!hasPermission && !hasGlobalPermission)
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
                else if (hasPermission && ownOnly && user.Id != currentUserId)
                {
                    response = ResponseBase<UserDTO>.Fail("Non sei autorizzato a visualizzare questo utente.", ErrorCode.Unauthorized);
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

    public async Task<ResponseBase<bool>> DeleteUserAsync(int userId)
    {
        var response = new ResponseBase<bool>();

        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<bool>.Fail("Utente non trovato.", ErrorCode.NotFound);
            }
            else
            {
                var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanDeleteUser");
                var currentUserId = _authorizationService.GetCurrentUserId();

                if (!hasPermission)
                {
                    response = ResponseBase<bool>.Fail("Non sei autorizzato a eliminare questo utente.", ErrorCode.Unauthorized);
                }
                else if (ownOnly && user.Id != currentUserId)
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

    public async Task<ResponseBase<UserDTO>> UpdateUserAsync(UserDTO userDto)
    {
        var response = new ResponseBase<UserDTO>();

        try
        {
            var user = await _repository.GetByIdAsync(userDto.Id);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<UserDTO>.Fail("Utente non trovato.", ErrorCode.NotFound);
            }
            else
            {
                var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userDto.Id, "CanUpdateUser");
                var currentUserId = _authorizationService.GetCurrentUserId();

                if (!hasPermission)
                {
                    response = ResponseBase<UserDTO>.Fail("Non sei autorizzato a modificare questo utente.", ErrorCode.Unauthorized);
                }
                else if ((ownOnly && user.Id != currentUserId))
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
                        var existingUser = await _repository.GetByEmailAsync(userDto.Email);
                        if (existingUser != null)
                        {
                            response = ResponseBase<UserDTO>.Fail("Email già in uso.", ErrorCode.ValidationError);
                            return response;
                        }
                        user.Email = userDto.Email;
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

    public async Task<ResponseBase<UserDTO>> GetCurrentUserAsync()
    {
        var response = new ResponseBase<UserDTO>();

        try
        {
            var userId = _authorizationService.GetCurrentUserId();
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
        catch (Exception ex)
        {
            response = ResponseBase<UserDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public async Task<ResponseBase<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest changePassword)
    {
        var response = new ResponseBase<bool>();

        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<bool>.Fail("Utente non trovato.", ErrorCode.NotFound);
            }
            else
            {
                var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanChangePassword");
                var currentUserId = _authorizationService.GetCurrentUserId();

                if (!hasPermission)
                {
                    response = ResponseBase<bool>.Fail("Non sei autorizzato a modificare la password di questo utente.", ErrorCode.Unauthorized);
                }
                else if (ownOnly && user.Id != currentUserId)
                {
                    response = ResponseBase<bool>.Fail("Non sei autorizzato a modificare la password di questo utente.", ErrorCode.Unauthorized);
                }
                else if (!_passwordService.VerifyPassword(user, user.PasswordHash, changePassword.OldPassword))
                {
                    response = ResponseBase<bool>.Fail("La password attuale non è corretta.", ErrorCode.ValidationError);
                }
                else if (string.IsNullOrWhiteSpace(changePassword.NewPassword))
                {
                    response = ResponseBase<bool>.Fail("La nuova password non può essere vuota.", ErrorCode.ValidationError);
                }
                else
                {
                    user.PasswordHash = _passwordService.HashPassword(user, changePassword.NewPassword);
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

    public async Task<ResponseBase<bool>> UpdateUserRolesAsync(int userId, UpdateRolesRequest request)
    {
        var response = new ResponseBase<bool>();

        try
        {
            // 1. Recupera l'utente
            var user = await _repository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<bool>.Fail("Utente non trovato.", ErrorCode.NotFound);
            }
            else
            {
                // 2. Ottiene i ruoli attuali
                var currentRoles = await _repository.GetUserRoleNamesAsync(userId);

                // 3. Blocco protezione admin
                if (currentRoles.Contains(RoleNames.Admin))
                {
                    response = ResponseBase<bool>.Fail("Non puoi modificare un utente admin.", ErrorCode.Unauthorized);
                }
                else
                {
                    // 4. Rimozione ruoli esistenti
                    await _repository.RemoveAllUserRolesAsync(userId);

                    // 5. Prepara i nuovi ruoli
                    var incomingRoles = request.Roles
                        .Where(r => r != RoleNames.Admin)
                        .Distinct()
                        .ToList();

                    if (!incomingRoles.Contains(RoleNames.Client))
                        incomingRoles.Add(RoleNames.Client);

                    // 6. Assegna i nuovi ruoli
                    var roleIds = await _roleRepository.GetRoleIdsByNamesAsync(incomingRoles);
                    await _repository.AddUserRolesAsync(userId, roleIds);

                    // 7. Rimozione precedente associazione fornitore (se esistente)
                    await _supplierUserRepository.RemoveAllSupplierUsersByUserIdAsync(userId);

                    // 8. Se il nuovo ruolo include usersupplier, assegna il fornitore
                    if (incomingRoles.Contains(RoleNames.UserSupplier))
                    {
                        if (request.IdSupplier == null)
                        {
                            response = ResponseBase<bool>.Fail("ID fornitore richiesto per assegnare usersupplier.", ErrorCode.ValidationError);
                            return response;
                        }

                        await _supplierUserRepository.AddSupplierUserAsync(userId, request.IdSupplier.Value);
                    }

                    // 9. Successo
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

    public async Task<ResponseBase<bool>> UpgradeToBusinessAsync(UpgradeToBusinessRequest request)
    {
        var response = new ResponseBase<bool>();

        try
        {
            var userId = _authorizationService.GetCurrentUserId(); // ✅ recupero centralizzato

            var user = await _repository.GetByIdWithRoleAsync(userId);
            if (user == null || user.IsDeleted)
            {
                response = ResponseBase<bool>.Fail("Utente non trovato.", ErrorCode.NotFound);
            }
            else
            {
                var roleNames = await _repository.GetUserRoleNamesAsync(userId);

                // 1. Aggiungi ruolo usersupplier se non presente
                if (!roleNames.Contains(RoleNames.UserSupplier))
                {
                    var publicRoles = await _roleRepository.GetPublicRolesAsync();
                    var userSupplierRole = publicRoles.FirstOrDefault(r => r.Name == RoleNames.UserSupplier);

                    if (userSupplierRole == null)
                    {
                        response = ResponseBase<bool>.Fail("Ruolo usersupplier non disponibile.", ErrorCode.ValidationError);
                    }
                    else
                    {
                        await _repository.AddUserRolesAsync(userId, new List<int> { userSupplierRole.Id });

                        foreach (var supplierId in request.IdSuppliers.Distinct())
                        {
                            var exists = await _supplierUserRepository.ExistsAsync(userId, supplierId);
                            if (!exists)
                            {
                                await _supplierUserRepository.AddSupplierUserAsync(userId, supplierId);
                            }
                        }

                        response = ResponseBase<bool>.Success(true);
                    }
                }
                else
                {
                    // 2. Già usersupplier: aggiungi solo i fornitori
                    foreach (var supplierId in request.IdSuppliers.Distinct())
                    {
                        var exists = await _supplierUserRepository.ExistsAsync(userId, supplierId);
                        if (!exists)
                        {
                            await _supplierUserRepository.AddSupplierUserAsync(userId, supplierId);
                        }
                    }

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
