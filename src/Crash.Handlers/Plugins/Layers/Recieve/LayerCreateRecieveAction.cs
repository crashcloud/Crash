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
			return change.HasFlag(ChangeAction.Add);
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
				RhinoLayerUtils.MoveLayerToCorrectLocation(rhinoLayer, crashLayer);

				var layerTable = args.Doc.Tables.Get<LayerTable>();
				layerTable.UpdateLayer(crashLayer);

				rhinoDoc.Layers.Add(rhinoLayer);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}
	}
}
