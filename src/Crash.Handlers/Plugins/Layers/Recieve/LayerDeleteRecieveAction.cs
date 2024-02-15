using System.Text.Json;

using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;

using Rhino.DocObjects;

namespace Crash.Handlers.Plugins.Layers.Recieve
{
	public class LayerDeleteRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.HasFlag(ChangeAction.Remove);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			crashDoc.Queue.AddAction(new IdleAction(DeleteLayerAction, new IdleArgs(crashDoc, recievedChange)));
		}

		private void DeleteLayerAction(IdleArgs args)
		{
			var packet = JsonSerializer.Deserialize<PayloadPacket>(args.Change.Payload);
			var layerUpdates = packet.Updates;

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

			if (!layerUpdates.TryGetValue(nameof(Layer.FullPath), out var fullPath))
			{
				return;
			}

			var layerIndex = rhinoDoc.Layers.FindByFullPath(fullPath, -1);
			rhinoDoc.Layers.Delete(layerIndex, false);

			args.Doc.RealisedChangeTable.DeleteChange(args.Change.Id);
		}
	}
}
