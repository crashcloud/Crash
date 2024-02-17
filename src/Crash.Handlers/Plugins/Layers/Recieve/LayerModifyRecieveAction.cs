using System.Text.Json;

using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers.Plugins.Layers.Recieve
{
	public class LayerModifyRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.HasFlag(ChangeAction.Update);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			crashDoc.Queue.AddAction(new IdleAction(ModifyLayerAction, new IdleArgs(crashDoc, recievedChange)));
		}

		private void ModifyLayerAction(IdleArgs args)
		{
			var packet = JsonSerializer.Deserialize<PayloadPacket>(args.Change.Payload);
			var layerUpdates = packet.Updates;

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

			args.Doc.DocumentIsBusy = true;
			try
			{
				Layer layer = null;
				if (!RhinoLayerUtils.TryGetAtExpectedPath(rhinoDoc, layerUpdates, out layer))
				{
					// Full Path is now different!
					if (!RhinoLayerUtils.TryGetAtOldPath(rhinoDoc, layerUpdates, out layer))
					{
						return;
					}

					// Move Layer to New Full Path!
					layer = RhinoLayerUtils.MoveLayerToExpectedPath(rhinoDoc, layerUpdates);
				}

				// Handle New Layer Full Path
				RhinoLayerUtils.UpdateLayer(layer, layerUpdates);

				if (!layer.HasIndex)
				{
					var newIndex = rhinoDoc.Layers.Add(layer);
					layer = rhinoDoc.Layers.FindIndex(newIndex);
				}

				args.Doc.RealisedChangeTable.AddPair(args.Change.Id, layer.Id);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}

		private static void ChangedFullPath(Dictionary<string, string> updates, RhinoDoc rhinoDoc)
		{
			if (!updates.TryGetValue(RhinoLayerUtils.GetOldKey(nameof(Layer.FullPath)), out var oldFullPath))
			{
				return;
			}

			if (!updates.TryGetValue(RhinoLayerUtils.GetNewKey(nameof(Layer.FullPath)), out var newFullPath))
			{
				return;
			}

			var layerIndex = rhinoDoc.Layers.FindByFullPath(oldFullPath, -1);
			if (layerIndex <= 0)
			{
				return;
			}

			var layer = rhinoDoc.Layers.FindIndex(layerIndex);
			if (layer is null)
			{
				return;
			}

			RhinoLayerUtils.SetLayerFullPath(rhinoDoc, layer, newFullPath);
		}
	}
}
