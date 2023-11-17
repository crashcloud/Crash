namespace Crash.Common.Changes
{
	public static class DoneChange
	{
		public const string ChangeType = "Crash.DoneChange";

		public static Change GetDoneChange(string owner)
		{
			return new Change
			       {
				       Stamp = DateTime.UtcNow,
				       Id = Guid.NewGuid(),
				       Owner = owner,
				       Action = ChangeAction.Release,
				       Type = ChangeType
			       };
		}
	}
}
