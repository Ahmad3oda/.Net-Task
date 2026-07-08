using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(new { message = "Success", data = users });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = "User not found" });

        return Ok(new { message = "Success", data = user });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return Ok(new { message = "Success", data = user });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        return Ok(new { message = "Success", data = user });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(new { message = "Success", data = new { } });
    }

    [HttpGet("me")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetMe()
    {
        var userIdString = User.FindFirstValue("UserId");
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound(new { message = "User not found" });

        return Ok(new { message = "Success", data = user });
    }
}
