using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;

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
			var crashLayer = CrashLayer.CreateFrom(args.Change);

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

			args.Doc.DocumentIsBusy = true;
			try
			{
				var layerTable = args.Doc.Tables.Get<LayerTable>();
				layerTable.MarkAsDeleted(crashLayer.Index);

				rhinoDoc.Layers.Delete(crashLayer.Index, false);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}
	}
}
