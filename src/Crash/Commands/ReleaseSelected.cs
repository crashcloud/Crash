using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers;
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

			await crashDoc.LocalClient.PushIdenticalChangesAsync(selectedChanges,
																 DoneChange.GetDoneChange(string.Empty));

			doc.Objects.UnselectAll();
			doc.Views.Redraw();

			return Result.Success;
		}

		private static IEnumerable<Guid> GetSelectedChanges(RhinoDoc doc)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			if (!crashDoc.Tables.TryGet<RealisedChangeTable>(out var realTable))
			{
				return Enumerable.Empty<Guid>();
			}

			var selected = new List<Guid>();
			foreach (var rhinoObj in doc.Objects.GetSelectedObjects(false, false))
			{
				if (!realTable.TryGetChangeId(rhinoObj.Id, out var changeId))
				{
					continue;
				}

				selected.Add(changeId);
			}

			return selected;
		}
	}
}
