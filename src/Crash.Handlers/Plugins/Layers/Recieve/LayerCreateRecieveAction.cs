using System.Text.Json;

using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Handlers.Utils;

using Rhino.DocObjects;

namespace Crash.Handlers.Plugins.Layers.Recieve
{
	public class LayerCreateRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.HasFlag(ChangeAction.Add);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			crashDoc.Queue.AddAction(new IdleAction(CreateLayerAction, new IdleArgs(crashDoc, recievedChange)));
		}

		private void CreateLayerAction(IdleArgs args)
		{
			var packet = JsonSerializer.Deserialize<PayloadPacket>(args.Change.Payload);
			var layerUpdates = packet.Updates;

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

			args.Doc.DocumentIsBusy = true;
			try
			{
				var userName = args.Doc.Users.CurrentUser.Name;
				if (!RhinoLayerUtils.TryGetAtExpectedPath(rhinoDoc, layerUpdates, userName, out var layer))
				{
					layer = new Layer();
				}

				if (!layer.HasIndex)
				{
					var newIndex = rhinoDoc.Layers.Add(layer);
					layer = rhinoDoc.Layers.FindIndex(newIndex);
				}

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
