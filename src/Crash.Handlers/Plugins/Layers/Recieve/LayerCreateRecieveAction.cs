using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Handlers.Utils;

namespace Crash.Handlers.Plugins.Layers.Recieve
{
	public class LayerCreateRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			// TODO : Add Deleted Layers and Delete them so Undo works?
			return change.HasFlag(ChangeAction.Add) && !change.HasFlag(ChangeAction.Remove);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			crashDoc.Queue.AddAction(new IdleAction(CreateLayerAction, new IdleArgs(crashDoc, recievedChange)));
		}

		private void CreateLayerAction(IdleArgs args)
		{
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

			args.Doc.DocumentIsBusy = true;
			try
			{
				var crashLayer = CrashLayer.CreateFrom(args.Change);

				var rhinoLayer = crashLayer.GetOrCreateRhinoLayer(rhinoDoc);
				rhinoLayer = RhinoLayerUtils.MoveLayerToCorrectLocation(rhinoDoc, rhinoLayer, crashLayer);
				CrashLayer.UpdateRegisteredLayer(rhinoDoc, crashLayer);

				var layerTable = args.Doc.Tables.Get<LayerTable>();
				layerTable.UpdateLayer(crashLayer);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}
	}
}
