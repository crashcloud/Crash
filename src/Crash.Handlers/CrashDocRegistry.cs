using BidirectionalMap;

using Crash.Common.Document;
using Crash.Common.Events;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers
{
	/// <summary>Stores the active <see cref="CrashDoc" /> /  <see cref="RhinoDoc" /> Pairs</summary>
	public static class CrashDocRegistry
	{
		private static readonly BiMap<RhinoDoc, CrashDoc> DocumentRelationship;

		static CrashDocRegistry()
		{
			DocumentRelationship = new BiMap<RhinoDoc, CrashDoc>();
		}

		/// <summary>The Active Crash Document.</summary>
		[Obsolete("Don't use this, it's being phased out")]
		public static CrashDoc? ActiveDoc => GetRelatedDocument(RhinoDoc.ActiveDoc);

		/// <summary>
		///     Returns the Document Related to the given <see cref="RhinoDoc" />
		/// </summary>
		/// <param name="doc">The <see cref="RhinoDoc" /> to check for a related <see cref="CrashDoc" /></param>
		/// <returns>A <see cref="CrashDoc" /> if any found, null otherwise</returns>
		public static CrashDoc? GetRelatedDocument(RhinoDoc doc)
		{
			if (doc is null)
			{
				return null;
			}

			if (DocumentRelationship.Forward.ContainsKey(doc))
			{
				return DocumentRelationship.Forward[doc];
			}

			return null;
		}

		/// <summary>
		///     Returns the Document Related to the given <see cref="CrashDoc" />
		/// </summary>
		/// <param name="doc">The <see cref="CrashDoc" /> to check for a related <see cref="RhinoDoc" /></param>
		/// <returns>A <see cref="RhinoDoc" /> if any found, null otherwise</returns>
		public static RhinoDoc? GetRelatedDocument(CrashDoc doc)
		{
			foreach (var kvp in DocumentRelationship.Reverse)
			{
				if (kvp.Key.Equals(doc))
				{
					return kvp.Value;
				}
			}

			return null;
		}

		/// <summary>
		///     Returns all of the open <see cref="CrashDoc" />s. Closed Documents are discarded.
		/// </summary>
		public static IEnumerable<CrashDoc> GetOpenDocuments()
		{
			return DocumentRelationship.Forward.Values;
		}

		/// <summary>
		///     Creates a new <see cref="CrashDoc" /> and registers it to the Registry.
		/// </summary>
		/// <returns>The new <see cref="CrashDoc" /></returns>
		public static CrashDoc CreateAndRegisterDocument(RhinoDoc rhinoDoc)
		{
			if (DocumentRelationship.Forward.ContainsKey(rhinoDoc))
			{
				return DocumentRelationship.Forward[rhinoDoc];
			}

			var crashDoc = new CrashDoc();
			Register(crashDoc, rhinoDoc);
			DocumentRegistered?.Invoke(null, new CrashEventArgs(crashDoc));

			crashDoc.Queue.OnCompletedQueue += RedrawOncompleted;
			crashDoc.LocalClient.OnInit += RegisterQueue;

			return crashDoc;
		}

		private static void RegisterQueue(object? sender, CrashInitArgs e)
		{
			RhinoApp.WriteLine("Loading Changes ...");

			// TODO : How to deregister?
			RhinoApp.Idle += (o, args) =>
			                 {
				                 e.CrashDoc.Queue.RunNextAction();
			                 };
		}

		private static void RedrawOncompleted(object? sender, CrashEventArgs e)
		{
			var rhinoDoc = GetRelatedDocument(e.CrashDoc);
			rhinoDoc.Views.Redraw();
		}

		private static void Register(CrashDoc crashDoc,
			RhinoDoc rhinoDoc)
		{
			DocumentRelationship.Add(rhinoDoc, crashDoc);
		}

		/// <summary>
		///     Disposes of the given <see cref="CrashDoc" /> and also disconnects any active connections.
		/// </summary>
		public static async Task DisposeOfDocumentAsync(CrashDoc crashDoc)
		{
			crashDoc.Queue.ForceCycleQueue();
			// DeRegister Events
			crashDoc.Queue.OnCompletedQueue -= RedrawOncompleted;
			if (crashDoc.LocalClient is not null)
			{
				crashDoc.LocalClient.OnInit -= RegisterQueue;
				await crashDoc.LocalClient?.StopAsync();
			}

			// Remove Geometry
			var rhinoDoc = GetRelatedDocument(crashDoc);
			DocumentRelationship.Remove(rhinoDoc);

			var settings = new ObjectEnumeratorSettings
			               {
				               ActiveObjects = false, LockedObjects = true, HiddenObjects = true
			               };
			var rhinoObjects = rhinoDoc.Objects.GetObjectList(settings);
			foreach (var rhinoObject in rhinoObjects)
			{
				rhinoDoc.Objects.Unlock(rhinoObject, true);
				rhinoDoc.Objects.Show(rhinoObject, true);
			}

			rhinoDoc.Objects.Clear();

			// Dispose
			crashDoc?.Dispose();
			DocumentDisposed?.Invoke(null, new CrashEventArgs(crashDoc));
		}

		/// <summary>
		///     Fired when a new <see cref="CrashDoc" /> is registered
		/// </summary>
		public static event EventHandler<CrashEventArgs> DocumentRegistered;

		/// <summary>
		///     Fired when a <see cref="CrashDoc" /> is disposed of
		/// </summary>
		public static event EventHandler<CrashEventArgs> DocumentDisposed;
	}
}
