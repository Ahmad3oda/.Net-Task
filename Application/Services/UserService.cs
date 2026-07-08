using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existingUsername != null) throw new Exception("Username already exists");

        var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingEmail != null) throw new Exception("Email already exists");

        var userRole = await _userRepository.GetRoleByNameAsync("User");
        if (userRole == null) throw new Exception("Role 'User' not found");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = userRole.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        
        user.Role = userRole; // For mapping
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new Exception("User not found");

        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUsername != null) throw new Exception("Username already exists");
            user.Username = dto.Username;
        }

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null) throw new Exception("Email already exists");
            user.Email = dto.Email;
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new Exception("User not found");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role?.Name ?? string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
