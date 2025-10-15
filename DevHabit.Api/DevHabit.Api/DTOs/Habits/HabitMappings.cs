using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

internal static class HabitMappings
{
    public static HabitDto ToDto(this Habit habit)
    {
        return new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            MileStone = habit.MileStone == null ? null : new MileStoneDto
            {
                Target = habit.MileStone.Target,
                Current = habit.MileStone.Current
            },
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc
        };
    }
    public static Habit ToEntity(this CreateHabitDto createHabitDto)
    {
        var habit = new Habit
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = createHabitDto.Name,
            Description = createHabitDto.Description,
            Type = createHabitDto.Type,
            Frequency = new Frequency
            {
                Type = createHabitDto.Frequency.Type,
                TimesPerPeriod = createHabitDto.Frequency.TimesPerPeriod
            },
            Target = new Target
            {
                Value = createHabitDto.Target.Value,
                Unit = createHabitDto.Target.Unit
            },
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            EndDate = createHabitDto.EndDate,
            MileStone = createHabitDto.MileStone == null ? null : new MileStone
            {
                Target = createHabitDto.MileStone.Target,
                Current = 0
            },
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = null,
            LastCompletedAtUtc = null
        };
        return habit;
    }

    public static HabitDto UpdateFromDto(this Habit habit, UpdateHabitDto dto)
    {
        habit.Name = dto.Name;
        habit.Description = dto.Description;
        habit.Type = dto.Type;
        habit.EndDate = dto.EndDate;
        habit.Frequency = new Frequency
        {
            Type = dto.Frequency.Type,
            TimesPerPeriod = dto.Frequency.TimesPerPeriod
        }
        ;
        habit.Target = new Target
        {
            Value = dto.Target.Value,
            Unit = dto.Target.Unit
        };
        if (dto.MileStone != null)
        {
            habit.MileStone ??= new MileStone();
            habit.MileStone.Target = dto.MileStone.Target;
        }

        habit.UpdatedAtUtc = DateTime.UtcNow;
        return habit.ToDto();
    }
}
