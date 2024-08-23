using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")] // api/UserController
public class UserController(DataContext context) : ControllerBase
{
  [HttpGet] // api/User
  public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
  {
    var users = await context.Users.ToListAsync();

    return users;
  }

  [HttpGet("{id:int}")] // api/User/123
  public async Task<ActionResult<AppUser>> GetUser(int id)
  {
    var user = await context.Users.FindAsync(id);
   
    if (user == null) return NotFound();

    return user;
  }
}
