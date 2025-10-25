using DevHabit.Api.Database;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits/{habitId}/tags")]
public sealed class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPut()]
    public async Task<ActionResult> UpsertHabitTags(string habitId, UpsertHabitTagsDto upsertHabitTagsDto)
    {
        Habit? habit = await dbContext.Habits
            .Include(habit => habit.HabitTags)
            .FirstOrDefaultAsync(habit => habit.Id == habitId);
        if (habit is null)
        {
            return NotFound();
        }

        var currentTagIds = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();
        if (currentTagIds.SetEquals(upsertHabitTagsDto.TagIds))
        {
            return NoContent();
        }
        List<string> existingTagIds = await dbContext
            .Tags
            .Where(tag => upsertHabitTagsDto.TagIds.Contains(tag.Id))
            .Select(tag => tag.Id)
            .ToListAsync();

        if (existingTagIds.Count != upsertHabitTagsDto.TagIds.Count)
        {
            return BadRequest("One or more TagIds do not exist.");
        }

        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));
        habit.HabitTags.AddRange(
            upsertHabitTagsDto.TagIds
                .Where(tagId => !currentTagIds.Contains(tagId))
                .Select(tagId => new HabitTag
                {
                    HabitId = habitId,
                    TagId = tagId,
                    CreatedAtUtc = DateTime.UtcNow
                })
        );
        await dbContext.SaveChangesAsync();

        return Ok();

    }
    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTag(string habitId, string tagId)
    {
        HabitTag? habitTag = await dbContext.HabitTags
            .SingleOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);

        if (habitTag is null)
        {
            return NotFound();
        }
        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();
        return NoContent();

    }
}
