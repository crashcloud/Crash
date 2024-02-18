using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Layers.Create
{
	public class LayerModifyAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Update;

		public bool TryConvert(object sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
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
				                                   layerArgs.Action,
				                                   layerArgs.Updates)
			          };

			var layerTable = crashArgs.Doc.Tables.Get<LayerTable>();
			layerTable.UpdateLayer(layerArgs.CrashLayer);

			return true;
		}

		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashLayerArgs cargs && cargs.Action == Action;
		}
	}
}
