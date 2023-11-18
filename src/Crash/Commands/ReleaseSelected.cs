﻿using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Utils;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class ReleaseSelected : AsyncCommand
	{
		public ReleaseSelected()
		{
			Instance = this;
		}


		public static ReleaseSelected Instance { get; private set; }


		public override string EnglishName => "ReleaseSelected";

		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc crashDoc, RunMode mode)
		{
			if (!CommandUtils.InSharedModel(crashDoc))
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return Result.Failure;
			}

			var selectedChanges = GetSelectedChanges(doc);
			if (!selectedChanges.Any())
			{
				return Result.Cancel;
			}

			var user = crashDoc.Users.CurrentUser.Name;
			await crashDoc.LocalClient.PushIdenticalChangesAsync(selectedChanges, DoneChange.GetDoneChange(user));

			return Result.Success;
		}

		private static IEnumerable<Guid> GetSelectedChanges(RhinoDoc doc)
		{
			foreach (var rhinoObj in doc.Objects.GetSelectedObjects(false, false))
			{
				if (!rhinoObj.TryGetChangeId(out var id))
				{
					continue;
				}

				yield return id;
			}
		}
	}
}
