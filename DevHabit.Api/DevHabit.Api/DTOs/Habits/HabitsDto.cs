using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;


public sealed record HabitsCollectionDto
{
    public List<HabitDto> Data { get; init; }
}
public sealed record HabitDto
{
    public required string Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public HabitType Type { get; init; }
    public FrequencyDto Frequency { get; init; }
    public TargetDto Target { get; init; }
    public HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MileStoneDto? MileStone { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAtUtc { get; init; }

}

public sealed record TargetDto
{
    public required int Value { get; init; }
    public required string Unit { get; init; } = string.Empty;
}

public sealed record MileStoneDto
{
    public required int Target { get; init; }
    public required int Current { get; init; }
}
public sealed record FrequencyDto
{
    public required FrequencyType Type { get; init; }
    public required int TimesPerPeriod { get; init; }
}
