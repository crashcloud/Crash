using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Layers.Create
{
	public class LayerCreateAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Add;

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
												   ChangeAction.Add,
												   layerArgs.Updates)
					  };

			var layerTable = crashArgs.Doc.Tables.Get<LayerTable>();
			layerTable.AddLayer(layerArgs.CrashLayer);

			return true;
		}

		public bool CanConvert(object? sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashLayerArgs cargs && cargs.Action == Action;
		}
	}
}
