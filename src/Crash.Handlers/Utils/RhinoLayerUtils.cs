using Crash.Handlers.Plugins.Layers;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers.Utils
{
	public static class RhinoLayerUtils
	{
		public static Layer MoveLayerToCorrectLocation(RhinoDoc rhinoDoc, Layer rhinoLayer, CrashLayer crashLayer)
		{
			var layerResult = rhinoLayer;
			if (!rhinoLayer.HasIndex)
			{
				var layerIndex = rhinoDoc.Layers.Add(rhinoLayer);
				layerResult = rhinoDoc.Layers.FindIndex(layerIndex);
			}

			if (layerResult.FullPath.Equals(crashLayer.FullPath))
			{
				return layerResult;
			}

			var lineage = CrashLayer.GetLayerLineage(crashLayer.FullPath).ToList();
			Layer previousLayer = null;
			for (var i = 1; i < lineage.Count + 1; i++)
			{
				var range = lineage.GetRange(0, i);
				var fullPath = CrashLayer.GetFullPath(range);

				var layerIndex = rhinoDoc.Layers.FindByFullPath(fullPath, -1);
				if (layerIndex == -1)
				{
					layerIndex = AddLayer(rhinoDoc, fullPath);
					if (previousLayer is not null)
					{
						AssignParent(rhinoDoc, previousLayer.Id, layerIndex);
					}
				}

				previousLayer = rhinoDoc.Layers.FindIndex(layerIndex);
			}

			return layerResult;
		}

		private static void AssignParent(RhinoDoc rhinoDoc, Guid parentId, int layerIndex)
		{
			var layer = rhinoDoc.Layers.FindIndex(layerIndex);
			layer.ParentLayerId = parentId;
		}

		private static int AddLayer(RhinoDoc rhinoDoc, string fullPath)
		{
			var newLayer = new Layer { Name = CrashLayer.GetLayerNameFromPath(fullPath) };
			return rhinoDoc.Layers.Add(newLayer);
		}
	}
}
