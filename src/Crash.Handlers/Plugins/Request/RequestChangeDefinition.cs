using Crash.Handlers.Plugins.Request.Recieve;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.Handlers.Plugins.Request
{
	/// <summary>
	///     Describes a Request Change, which lets users request currently temporary changes
	/// </summary>
	public sealed class RequestChangeDefinition : IChangeDefinition
	{
		public string ChangeName => RequestChange.ChangeType;
		public IEnumerable<IChangeCreateAction> CreateActions { get; }
		public IEnumerable<IChangeRecieveAction> RecieveActions { get; }
		
		public RequestChangeDefinition()
		{
			CreateActions = new List<IChangeCreateAction>();
			RecieveActions = new List<IChangeRecieveAction>()
			                 {
				                 new RequestRecieveAction()
			                 };
		}


		private BoundingBox CachedBox { get; set; } = BoundingBox.Empty;


		public void Draw(DrawEventArgs drawArgs, DisplayMaterial material, IChange change)
		{
			if (change is not RequestChange requestChange)
			{
				return;
			}

			var crashDoc = CrashDocRegistry.GetRelatedDocument(drawArgs.RhinoDoc);

			if (!crashDoc.RealisedChangeTable.TryGetRhinoId(requestChange.RequestedId, out var RhinoId))
			{
				return;
			}

			var rhinoObject = drawArgs.RhinoDoc.Objects.FindId(RhinoId);
			if (rhinoObject is null)
			{
				return;
			}

			CachedBox = rhinoObject.Geometry.GetBoundingBox(Plane.WorldXY);
			CachedBox.Inflate(1.1);

			drawArgs.Display.DrawBox(CachedBox, material.Diffuse, 5);

			var topLeft = CachedBox.PointAt(1, 1, 1);
			drawArgs.Display.DrawDot(topLeft, change.Owner);
		}

		public BoundingBox GetBoundingBox(IChange change)
		{
			return CachedBox;
		}
	}
}
