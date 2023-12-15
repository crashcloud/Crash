using BidirectionalMap;

using Crash.Common.Document;
using Crash.Common.Events;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers
{
	// TODO : Is this needed?
	public static class CrashDocRegistry
	{
		private static readonly BiMap<RhinoDoc, CrashDoc> DocumentRelationship;

		static CrashDocRegistry()
		{
			DocumentRelationship = new BiMap<RhinoDoc, CrashDoc>();
			RhinoDoc.ActiveDocumentChanged += RhinoDoc_ActiveDocumentChanged;
		}

		/// <summary>The Active Crash Document.</summary>
		public static CrashDoc? ActiveDoc => GetRelatedDocument(RhinoDoc.ActiveDoc);

		private static void RhinoDoc_ActiveDocumentChanged(object sender, DocumentEventArgs e)
		{
			// ... 
		}

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

		public static IEnumerable<CrashDoc> GetOpenDocuments()
		{
			return DocumentRelationship.Forward.Values;
		}

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

		public static async Task DisposeOfDocumentAsync(CrashDoc crashDoc)
		{
			crashDoc.Queue.ForceCycleQueue();
			DocumentDisposed?.Invoke(null, new CrashEventArgs(crashDoc));
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
		}

		public static event EventHandler<CrashEventArgs> DocumentRegistered;
		public static event EventHandler<CrashEventArgs> DocumentDisposed;
	}
}
