using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins.Layers;

namespace Crash.Handlers.Plugins.Camera.Create
{
	public class LayerCreateAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Add;

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
				                                   layerArgs.CrashLayer.ChangeId,
				                                   layerArgs.Action,
				                                   layerArgs.Updates)
			          };
			return true;
		}

		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashLayerArgs cargs && cargs.Action == Action;
		}
	}
}
