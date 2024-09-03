using Crash.Common.Document;
using Crash.Handlers.Data;

namespace Crash.UI;

internal class RecentModelViewModel : BaseViewModel
{
	public SharedModel Model { get; }

	public ModelRenderState State { get; private set; }

	private CrashDoc Doc { get; }

	private string UserName { get; }

	public RecentModelViewModel(SharedModel model)
	{
		Model = model;
		UserName = Guid.NewGuid().ToString();
		Doc = GetCrashDoc();

		State = Model switch
		{
			null => ModelRenderState.Add,
			_ => ModelRenderState.Loading;
			};
	}

	public async Task AttemptToConnect()
	{
		var client = Doc.LocalClient;
		client.RegisterConnection(UserName, new Uri(Model.ModelAddress));
		var result = await client.StartLocalClientAsync();
		State = result switch
		{
			null => ModelRenderState.Loaded,
			_ => ModelRenderState.FailedToLoad
		};
	}

	private CrashDoc GetCrashDoc()
	{
		var doc = new CrashDoc();
		doc.Users.CurrentUser = new User(UserName);
		return doc;
	}

}
