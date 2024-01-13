using Rhino.Display;
using Rhino.Geometry;

namespace Crash.Handlers.Plugins
{
	/// <summary>Describes a Change</summary>
	public interface IChangeDefinition
	{
		/// <summary>The Name to recognise these Changes by</summary>
		string ChangeName { get; }

		/// <summary>
		///     These will be registered somewhere and Crash will perform a fall through to find the first conversion candidate
		///     They will be index by Action too.
		/// </summary>
		IEnumerable<IChangeCreateAction> CreateActions { get; }

		/// <summary>
		///     These will be registered somewhere, and when Crash receives a Change, and then perform the conversion
		///     It will then be indexed by name
		/// </summary>
		IEnumerable<IChangeRecieveAction> RecieveActions { get; }

		/// <summary>Draws the Change in the Pipeline</summary>
		void Draw(DrawEventArgs drawArgs, DisplayMaterial material, IChange change);

		/// <summary>Returns a BoundingBox of the Change for Drawing</summary>
		BoundingBox GetBoundingBox(IChange change);
	}
}
