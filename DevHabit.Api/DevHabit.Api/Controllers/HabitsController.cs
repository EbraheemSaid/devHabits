using System.Dynamic;
using System.Linq.Dynamic.Core;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
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
public sealed class HabitsController(
    ApplicationDbContext dbContext,
    LinkService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitQueryParams queryParams,
        SortMappingsProvider sortMappingsProvider,
        DataShapingService dataShapingService)
    {

        if (!sortMappingsProvider
            .ValidateMappings<HabitDto, Habit>(queryParams.Sort))
        {
            return Problem(
                detail: $"One or more sort fields are invalid: {queryParams.Sort}",
                statusCode: StatusCodes.Status400BadRequest);
        }
        if (!dataShapingService
            .ValidateFields<HabitDto>(queryParams.Fields))
        {
            return Problem(
                detail: $"One or more data shaping fields are invalid: {queryParams.Fields}",
                statusCode: StatusCodes.Status400BadRequest);
        }

        queryParams.Search = queryParams.Search?.Trim()?.ToLower();

        SortMapping[] sortMappings = sortMappingsProvider.GetSortMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitsQuery = dbContext
            .Habits
            .Where(h => queryParams.Search == null ||
                        h.Name.ToLower().Contains(queryParams.Search) ||
                        h.Description != null &&
                        h.Description.ToLower().Contains(queryParams.Search))
            .Where(h => queryParams.Type == null || h.Type == queryParams.Type)
            .Where(h => queryParams.Status == null || h.Status == queryParams.Status)
            .ApplySort(queryParams.Sort, sortMappings)
            .Select(HabitQueries.HabitToDtoProjection());


        int totalCount = await habitsQuery.CountAsync();
        List<HabitDto> habits = await habitsQuery
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();


        bool includeLinks =
            !string.IsNullOrWhiteSpace(queryParams.Accept) &&
            queryParams.Accept.Contains(CustomMediaTypeName.Application.HateoasJson, StringComparison.OrdinalIgnoreCase);

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(
                habits,
                queryParams.Fields,
                includeLinks ? h => CreateLinksForHabit(h.Id, queryParams.Fields) : null),
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount,
        };

        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(
                queryParams,
                paginationResult.HasNextPage,
                paginationResult.HasPreviousPage);

        }


        return Ok(paginationResult);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabitById(
        string id,
        string? fields,
        [FromHeader(Name = "Accept")]
        string? accept,
        DataShapingService dataShapingService)
    {

        if (!dataShapingService
           .ValidateFields<HabitWithTagsDto>(fields))
        {
            return Problem(
                detail: $"One or more data shaping fields are invalid: {fields}",
                statusCode: StatusCodes.Status400BadRequest);
        }
        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto())
            .FirstOrDefaultAsync();

        if (habit == null)
        {
            return NotFound();
        }

        var shapedObject = dataShapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeName.Application.HateoasJson)
        {
            var links = CreateLinksForHabit(id, fields);
            shapedObject.TryAdd("links", links);

        }


        return Ok(shapedObject);

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

        var habitLinks = CreateLinksForHabit(habitDto.Id, null);
        habitDto.Links = habitLinks;

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

    private List<LinkDto> CreateLinksForHabits(
        HabitQueryParams habitQueryParams,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(
                endpointName: nameof(GetHabits),
                rel: "self",
                method: "GET",
                values: new
                {
                    page = habitQueryParams.Page,
                    pageSize = habitQueryParams.PageSize,
                    fields = habitQueryParams.Fields,
                    q = habitQueryParams.Search,
                    sort = habitQueryParams.Sort,
                    type = habitQueryParams.Type,
                    status = habitQueryParams.Status
                }),

                linkService.Create(nameof(CreateHabit), "create_habit", "POST")
        ];

        if (hasNextPage)
        {
            links.Add(
                linkService.Create(
                    endpointName: nameof(GetHabits),
                    rel: "next_page",
                    method: "GET",
                    values: new
                    {
                        page = habitQueryParams.Page + 1,
                        pageSize = habitQueryParams.PageSize,
                        fields = habitQueryParams.Fields,
                        q = habitQueryParams.Search,
                        sort = habitQueryParams.Sort,
                        type = habitQueryParams.Type,
                        status = habitQueryParams.Status
                    }));
        }
        if (hasPreviousPage)
        {
            links.Add(
                linkService.Create(
                    endpointName: nameof(GetHabits),
                    rel: "previous_page",
                    method: "GET",
                    values: new
                    {
                        page = habitQueryParams.Page - 1,
                        pageSize = habitQueryParams.PageSize,
                        fields = habitQueryParams.Fields,
                        q = habitQueryParams.Search,
                        sort = habitQueryParams.Sort,
                        type = habitQueryParams.Type,
                        status = habitQueryParams.Status
                    }));
        }
        return links;
    }
    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        List<LinkDto> links =
        [
            linkService.Create(
                endpointName: nameof(GetHabitById),
                rel: "self",
                method: "GET",
                values: new { id, fields },
                controller: "Habits"),
            linkService.Create(
                endpointName: nameof(UpdateHabit),
                rel: "update_habit",
                method: "PUT",
                values: new { id },
                controller: "Habits"),
            linkService.Create(
                endpointName: nameof(DeleteHabit),
                rel: "delete_habit",
                method: "DELETE",
                values: new { id },
                controller: "habits"),
            linkService.Create(
                endpointName: nameof(PatchHabit),
                rel: "patch_habit",
                method: "PATCH",
                values: new { id },
                controller: "Habits"),
            linkService.Create(
                endpointName: nameof(HabitTagsController.UpsertHabitTags),
                rel: "upsert_habit_tags",
                method: "PUT",
                values: new { habitId = id },
                HabitTagsController.Name),

        ];

        return links;
    }
}


