namespace IronHell.Core.Characters;

public static class StatusLifecycleService
{
    public static StatusLifecycleResult AdvanceStatuses(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);

        var advanced = new List<string>();
        var expired = new List<string>();
        foreach (var status in character.ActiveStatuses.ToArray())
        {
            if (status.RemainingDuration <= 0)
            {
                character.RemoveStatus(status);
                expired.Add(status.StatusId);
                continue;
            }

            status.RemainingDuration--;
            advanced.Add(status.StatusId);
            if (status.RemainingDuration == 0)
            {
                character.RemoveStatus(status);
                expired.Add(status.StatusId);
            }
        }

        return new StatusLifecycleResult(advanced.AsReadOnly(), expired.AsReadOnly());
    }
}