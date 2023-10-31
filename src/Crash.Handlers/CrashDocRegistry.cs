using BidirectionalMap;

using Crash.Common.Document;

using Rhino;

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

			crashDoc.Queue.OnCompletedQueue += (s, e) =>
			                                   {
				                                   rhinoDoc.Views.Redraw();
			                                   };

			return crashDoc;
		}

		private static void Register(CrashDoc crashDoc,
			RhinoDoc rhinoDoc)
		{
			DocumentRelationship.Add(rhinoDoc, crashDoc);
		}

		public static void DisposeOfDocument(CrashDoc crashDoc)
		{
			var rhinoDoc = GetRelatedDocument(crashDoc);
			DocumentRelationship.Remove(rhinoDoc);
			crashDoc?.LocalClient?.StopAsync();
			crashDoc?.Dispose();
			rhinoDoc.Objects.Clear();
		}
	}
}
