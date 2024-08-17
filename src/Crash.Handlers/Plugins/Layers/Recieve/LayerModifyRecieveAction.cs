using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Handlers.Utils;

namespace Crash.Handlers.Plugins.Layers.Recieve
{
	public class LayerModifyRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.HasFlag(ChangeAction.Update) && !change.HasFlag(ChangeAction.Remove);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			crashDoc.Queue.AddAction(new IdleAction(ModifyLayerAction, new IdleArgs(crashDoc, recievedChange)));
		}

		private void ModifyLayerAction(IdleArgs args)
		{
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

			args.Doc.DocumentIsBusy = true;
			try
			{
				var crashLayer = CrashLayer.CreateFrom(args.Change);

				var rhinoLayer = crashLayer.GetOrCreateRhinoLayer(rhinoDoc);
				rhinoLayer = RhinoLayerUtils.MoveLayerToCorrectLocation(rhinoDoc, rhinoLayer, crashLayer);

				CrashLayer.UpdateRhinoLayer(rhinoDoc, crashLayer, rhinoLayer);
				CrashLayer.UpdateRegisteredLayer(rhinoDoc, crashLayer);

				crashLayer = new CrashLayer(rhinoLayer, args.Change.Id);
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
