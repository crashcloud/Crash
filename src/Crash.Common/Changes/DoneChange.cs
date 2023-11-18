namespace Crash.Common.Changes
{
	/// <summary>Represents a Done Change inside of Crash</summary>
	public static class DoneChange
	{
		public const string ChangeType = "Crash.DoneChange";

		/// <summary>Creates a Done Change Template</summary>
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
