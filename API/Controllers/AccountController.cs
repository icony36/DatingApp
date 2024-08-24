using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
  [HttpPost("register")] // api/account/register
  public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) // if the parameter is an object, it will use request body, if it is string, it will use query string
  {
    if (await UserExists(registerDto.Username)) return BadRequest("Username is taken.");

    using var hmac = new HMACSHA512(); // use using so the variable will be disposed once this class is out of scope

    var user = new AppUser
    {
      Username = registerDto.Username.ToLower(),
      PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
      PasswordSalt = hmac.Key
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();

    return new UserDto
    {
      Username = user.Username,
      Token = tokenService.CreateToken(user)
    };
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
  {
    var user = await context.Users.FirstOrDefaultAsync(x => x.Username == loginDto.Username.ToLower());

    if (user == null) return Unauthorized("Invalid username.");

    using var hmac = new HMACSHA512(user.PasswordSalt);

    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

    for (int i=0; i<computedHash.Length; i++)
    {
      if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password.");
    }

    return new UserDto
    {
      Username = user.Username,
      Token = tokenService.CreateToken(user)
    };
  }

  private async Task<bool> UserExists(string username)
  {
    return await context.Users.AnyAsync(x => x.Username.ToLower() == username.ToLower());
  }
}
