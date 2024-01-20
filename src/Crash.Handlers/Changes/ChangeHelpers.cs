using Crash.Common.Document;
using Crash.Handlers;

using Rhino.DocObjects;

using ChangeGuid = System.Guid;
using RhinoGuid = System.Guid;


namespace Crash.Utils
{
	/// <summary>Utilities for Change Objects.</summary>
	public static class ChangeHelpers
	{
		/// <summary>Acquires the Rhino Object given the RhinoId from an IRhinoChange</summary>
		public static bool TryGetRhinoObject(this IChange change, CrashDoc crashDoc, out RhinoObject rhinoObject)
		{
			rhinoObject = default;
			if (change is null || crashDoc is null)
			{
				return false;
			}

			if (!crashDoc.RealisedChangeTable.TryGetRhinoId(change, out var rhinoId))
			{
				return false;
			}

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
			if (rhinoDoc is null)
				return false;

			rhinoObject = rhinoDoc.Objects.FindId(rhinoId);

			return rhinoObject is not null;
		}
	}
}
