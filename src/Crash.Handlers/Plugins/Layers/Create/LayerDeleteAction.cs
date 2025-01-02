using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Layers.Create
{
	public class LayerDeleteAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Remove;

		public bool TryConvert(object? sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			changes = Array.Empty<Change>();
			if (crashArgs.Args is not CrashLayerArgs layerArgs)
			{
				return false;
			}

			var owner = crashArgs.Doc.Users.CurrentUser.Name;
			changes = new[]
			          {
				          LayerChange.CreateChange(owner,
				                                   layerArgs.CrashLayer,
				                                   ChangeAction.Remove,
				                                   layerArgs.Updates)
			          };

			var layerTable = crashArgs.Doc.Tables.Get<LayerTable>();
			layerTable.MarkAsDeleted(layerArgs.CrashLayer.Index);

			return true;
		}

		public bool CanConvert(object? sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashLayerArgs cargs && cargs.Action == Action;
		}
	}
}
