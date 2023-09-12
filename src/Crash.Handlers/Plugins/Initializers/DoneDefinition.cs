using Crash.Handlers.Plugins.Initializers.Recieve;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.Handlers.Plugins.Initializers
{
	/// <summary>Handles Done calls inside of Crash</summary>
	public sealed class DoneDefinition : IChangeDefinition
	{
		/// <summary>Default Constructor</summary>
		public DoneDefinition()
		{
			CreateActions = Array.Empty<IChangeCreateAction>();
			RecieveActions = new List<IChangeRecieveAction> { new DoneRecieve() };
		}

		public string ChangeName => "Crash.DoneChange";


		public IEnumerable<IChangeCreateAction> CreateActions { get; }


		public IEnumerable<IChangeRecieveAction> RecieveActions { get; }


		public void Draw(DrawEventArgs drawArgs, DisplayMaterial material, IChange change)
		{
			throw new NotImplementedException();
		}


		public BoundingBox GetBoundingBox(IChange change)
		{
			throw new NotImplementedException();
		}
	}
}
