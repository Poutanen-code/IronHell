namespace IronHell.Core.Characters;

public sealed class ActiveStatus
{
	public ActiveStatus(string statusId, int remainingDuration)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(statusId);
		if (remainingDuration < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(remainingDuration));
		}

		StatusId = statusId;
		RemainingDuration = remainingDuration;
	}

	public string StatusId { get; }

	public int RemainingDuration { get; set; }
}