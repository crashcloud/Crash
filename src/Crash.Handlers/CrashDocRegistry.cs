using Crash.Common.Document;
using Crash.Common.Events;

namespace Crash.Handlers
{
	/// <summary>
	///     Contains pairings of RhinoDocs and CrashDocs
	/// </summary>
	public static class CrashDocRegistry
	{
		private static readonly BiMap<RhinoDoc, CrashDoc> s_documentRelationship;

		static CrashDocRegistry()
		{
			s_documentRelationship = new BiMap<RhinoDoc, CrashDoc>();
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

			if (s_documentRelationship.Forward.ContainsKey(doc))
			{
				return s_documentRelationship.Forward[doc];
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
			foreach (var kvp in s_documentRelationship.Reverse)
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
			return s_documentRelationship.Forward.Values;
		}

		/// <summary>
		///     Creates a new <see cref="CrashDoc" /> and registers it to the Registry.
		/// </summary>
		/// <returns>The new <see cref="CrashDoc" /></returns>
		public static CrashDoc CreateAndRegisterDocument(RhinoDoc rhinoDoc)
		{
			if (s_documentRelationship.Forward.ContainsKey(rhinoDoc))
			{
				return s_documentRelationship.Forward[rhinoDoc];
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
			e.CrashDoc.LocalClient.OnInit -= RegisterQueue;
			RhinoApp.WriteLine("Loading Changes ...");

			EventHandler cycleQueueDelegate = null;
			cycleQueueDelegate = (o, args) =>
			                     {
				                     e.CrashDoc.Queue.RunNextAction();
			                     };
			RhinoApp.Idle += cycleQueueDelegate;

			EventHandler<CrashEventArgs> deRegisterQueueCycle = null;
			deRegisterQueueCycle = (o, args) =>
			                       {
				                       DocumentDisposed -= deRegisterQueueCycle;
				                       RhinoApp.Idle -= cycleQueueDelegate;
			                       };

			DocumentDisposed += deRegisterQueueCycle;
		}

		private static void CycleQueue(object sender, EventArgs e)
		{
		}

		private static void RedrawOncompleted(object? sender, CrashEventArgs e)
		{
			var rhinoDoc = GetRelatedDocument(e.CrashDoc);
			rhinoDoc.Views.Redraw();
		}

		private static void Register(CrashDoc crashDoc,
			RhinoDoc rhinoDoc)
		{
			s_documentRelationship.Add(rhinoDoc, crashDoc);
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
				await crashDoc.LocalClient?.StopAsync();
			}

			// Remove Geometry
			var rhinoDoc = GetRelatedDocument(crashDoc);
			s_documentRelationship.Remove(rhinoDoc);

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
