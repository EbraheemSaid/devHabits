using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

public sealed record CreateHabitDto
{
    public required string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public required HabitType Type { get; init; }
    public FrequencyDto Frequency { get; init; }
    public TargetDto Target { get; init; }
    public DateOnly? EndDate { get; init; }
    public MileStoneDto? MileStone { get; init; }
}
