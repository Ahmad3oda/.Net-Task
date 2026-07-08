using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task DeleteUserAsync(Guid id);
}

public interface IAuthService
{
    Task<string> LoginAsync(LoginDto dto);
    Task<UserDto> RegisterAsync(RegisterDto dto);
}
