using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public sealed class TagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetTags()
    {
        List<TagDto> tags = await dbContext
            .Tags
            .Select(TagQueries.TagToDtoProjection())
            .ToListAsync();
        var tagsCollectionDto = new TagsCollectionDto { Data = tags };
        return Ok(tagsCollectionDto);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult> GetTagById([FromRoute] string id)
    {
        TagDto? tag = await dbContext
            .Tags
            .Where(t => t.Id == id)
            .Select(TagQueries.TagToDtoProjection())
            .FirstOrDefaultAsync();

        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }
    [HttpPost]
    public async Task<ActionResult> CreateTag([FromBody] CreateTagDto createTagDto,
        IValidator<CreateTagDto> validator,
        ProblemDetailsFactory problemDetailsFactory

        )
    {
        ValidationResult validationResult = await validator.ValidateAsync(createTagDto);
        if (!validationResult.IsValid)
        {
            ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest
                );
            problemDetails.Extensions.Add("errors", validationResult.ToDictionary());
            return BadRequest(problemDetails);
        }
        var tag = createTagDto.ToEntity();

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Problem(
                 detail: $"The tag '{tag.Name}' already exists",
                statusCode: StatusCodes.Status409Conflict);
        }

        await dbContext.Tags.AddAsync(tag);
        await dbContext.SaveChangesAsync();

        var tagDto = TagsMappings.ToDto(tag);

        return CreatedAtAction(nameof(GetTagById), new { tagDto.Id }, tagDto);
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag([FromRoute] string id, [FromBody] UpdateTagDto updateTagDto)
    {
        Tag? existingTag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

        if (existingTag == null)
        {
            return NotFound();
        }

        existingTag.UpdateFromDto(updateTagDto);

        await dbContext.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag([FromRoute] string id)
    {
        Tag? existingTag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (existingTag == null)
        {
            return NotFound();
        }
        dbContext.Tags.Remove(existingTag);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
