using RhinoLayer = Rhino.DocObjects.Layer;

namespace Crash.Handlers.Plugins.Layers
{
	public class CrashLayer
	{
		internal CrashLayer(string fullName, int index)
		{
			FullName = fullName;
			Index = index;
		}

		internal CrashLayer(RhinoLayer layer, Guid changeId)
		{
			ChangeId = changeId;
			FullName = layer.FullPath;
			Index = layer.Index;
			IsDeleted = layer.IsDeleted;
			IsVisible = layer.IsVisible;
			IsLocked = layer.IsLocked;
			IsExpanded = layer.IsExpanded;
		}

		internal CrashLayer(Dictionary<string, string> properties)
		{
			// TODO : Implement
			throw new NotImplementedException();
		}

		public Guid ChangeId { get; set; }

		public string FullName { get; set; }
		public int Index { get; set; }

		public bool IsDeleted { get; set; }
		public bool IsVisible { get; set; }
		public bool IsLocked { get; set; }
		public bool IsExpanded { get; set; }
		public bool Current { get; set; }
	}
}
