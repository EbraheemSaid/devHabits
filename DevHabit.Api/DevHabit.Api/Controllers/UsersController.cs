using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id, CancellationToken cancellationToken)
    {
        UserDto? user = await dbContext.Users
             .Where(u => u.Id == id)
             .Select(UserQueries.ProjectToDto())
             .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}
