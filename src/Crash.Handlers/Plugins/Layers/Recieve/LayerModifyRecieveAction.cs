using System.Text.Json;

using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;

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

			var userName = args.Doc.Users.CurrentUser.Name;

			args.Doc.DocumentIsBusy = true;
			try
			{
				Layer layer = null;
				if (!RhinoLayerUtils.TryGetAtExpectedPath(rhinoDoc, layerUpdates, userName, out layer))
				{
					// Full Path is now different!
					if (!RhinoLayerUtils.TryGetAtOldPath(rhinoDoc, layerUpdates, userName, out layer))
					{
						return;
					}

					// Move Layer to New Full Path!
					layer = RhinoLayerUtils.MoveLayerToExpectedPath(rhinoDoc, layerUpdates, userName);
				}

				if (!layer.HasIndex)
				{
					var newIndex = rhinoDoc.Layers.Add(layer);
					layer = rhinoDoc.Layers.FindIndex(newIndex);
				}

				// Handle New Layer Full Path
				RhinoLayerUtils.UpdateLayer(layer, layerUpdates, userName);

				args.Doc.RealisedChangeTable.RestoreChange(args.Change.Id);
				args.Doc.RealisedChangeTable.AddPair(args.Change.Id, layer.Id);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}
	}
}
