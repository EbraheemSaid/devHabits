using System.Linq.Dynamic.Core;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;
[Route("Habits")]
[ApiController]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits(
        [FromQuery] HabitQueryParams queryParams,
        SortMappingsProvider sortMappingsProvider)
    {

        if (!sortMappingsProvider
            .ValidateMappings<HabitDto, Habit>(queryParams.Sort))
        {
            return Problem(
                detail: $"One or more sort fields are invalid: {queryParams.Sort}",
                statusCode: StatusCodes.Status400BadRequest);
        }

        queryParams.Search = queryParams.Search?.Trim()?.ToLower();

        SortMapping[] sortMappings = sortMappingsProvider.GetSortMappings<HabitDto, Habit>();

        List<HabitDto> habits = await dbContext
            .Habits
            .Where(h => queryParams.Search == null ||
                        h.Name.ToLower().Contains(queryParams.Search) ||
                        h.Description != null &&
                        h.Description.ToLower().Contains(queryParams.Search))
            .Where(h => queryParams.Type == null || h.Type == queryParams.Type)
            .Where(h => queryParams.Status == null || h.Status == queryParams.Status)
            .ApplySort(queryParams.Sort, sortMappings)
            .Select(HabitQueries.HabitToDtoProjection())
            .ToListAsync();

        var habitsCollectionDto = new HabitsCollectionDto { Data = habits };
        return Ok(habitsCollectionDto);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabitById(string id)
    {
        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto())
            .FirstOrDefaultAsync();

        if (habit == null)
        {
            return NotFound();
        }

        return Ok(habit);

    }
    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit([FromBody] CreateHabitDto createHabitDto,
        IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        var habit = createHabitDto.ToEntity();

        await dbContext.Habits.AddAsync(habit);
        await dbContext.SaveChangesAsync();

        var habitDto = HabitMappings.ToDto(habit);

        return CreatedAtAction(nameof(GetHabitById), new { habitDto.Id }, habitDto);

    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit([FromRoute] string id, [FromBody] UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(habit => habit.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();
        return NoContent();

    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(habit => habit.Id == id);

        if (habit is null)
        {
            return NotFound();
        }
        var habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }
        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;

        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(habit => habit.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
