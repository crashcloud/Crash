using Crash.Handlers.Plugins.Layers.Create;
using Crash.Handlers.Plugins.Layers.Recieve;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.Handlers.Plugins.Layers
{
	public class LayerChangeDefinition : IChangeDefinition
	{
		public LayerChangeDefinition()
		{
			CreateActions = new List<IChangeCreateAction> { new LayerCreateAction(), new LayerDeleteAction(), new LayerModifyAction() };
			RecieveActions = new List<IChangeRecieveAction>
			                 {
				                 new LayerCreateOrModifyRecieveAction(), new LayerDeleteRecieveAction()
			                 };
		}

		public string ChangeName => LayerChange.ChangeType;
		public IEnumerable<IChangeCreateAction> CreateActions { get; }
		public IEnumerable<IChangeRecieveAction> RecieveActions { get; }

		public void Draw(DrawEventArgs drawArgs, DisplayMaterial material, IChange change)
		{
		}

		public BoundingBox GetBoundingBox(IChange change)
		{
			return BoundingBox.Empty;
		}
	}
}
