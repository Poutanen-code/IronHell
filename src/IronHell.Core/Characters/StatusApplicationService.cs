using IronHell.Core.Definitions;

namespace IronHell.Core.Characters;

public static class StatusApplicationService
{
    public static StatusApplicationResult ApplyStatus(
        Character character,
        string statusId,
        int duration,
        StatusApplicationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentException.ThrowIfNullOrWhiteSpace(statusId);
        if (duration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        var existing = character.ActiveStatuses.FirstOrDefault(status => string.Equals(status.StatusId, statusId, StringComparison.Ordinal));
        if (existing is null)
        {
            var added = new ActiveStatus(statusId, duration);
            character.ApplyStatus(added);
            return new StatusApplicationResult(Applied: true, Replaced: false, added);
        }

        return policy switch
        {
            StatusApplicationPolicy.IgnoreIfPresent => new StatusApplicationResult(Applied: false, Replaced: false, existing),
            StatusApplicationPolicy.RefreshDuration => Refresh(existing, duration),
            StatusApplicationPolicy.ReplaceExisting => Replace(character, existing, statusId, duration),
            _ => throw new ArgumentOutOfRangeException(nameof(policy)),
        };
    }

    private static StatusApplicationResult Refresh(ActiveStatus existing, int duration)
    {
        existing.RemainingDuration = duration;
        return new StatusApplicationResult(Applied: true, Replaced: false, existing);
    }

    private static StatusApplicationResult Replace(Character character, ActiveStatus existing, string statusId, int duration)
    {
        character.RemoveStatus(existing);
        var replacement = new ActiveStatus(statusId, duration);
        character.ApplyStatus(replacement);
        return new StatusApplicationResult(Applied: true, Replaced: true, replacement);
    }
}