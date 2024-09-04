using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Handlers;
using Crash.Handlers.Data;

namespace Crash.UI;

internal class RecentModelViewModel : BaseViewModel
{
	public SharedModel Model { get; }

	private ModelRenderState _state { get; set; }
	public ModelRenderState State
	{
		get => _state;
		set
		{
			_state = value;
			NotifyPropertyChanged(nameof(State));
		}
	}

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
			_ => ModelRenderState.Loading,
		};
	}

	public async Task AttemptToConnect()
	{
		if (Model is null) return;

		// Makes a Docile client	
		var client = Doc.LocalClient = new CrashClient(Doc, new IClientOptions(true));
		try
		{
			client.RegisterConnection(UserName, new Uri($"{Model.ModelAddress}/Crash"));

			var result = await client.StartLocalClientAsync();
			State = result switch
			{
				null => ModelRenderState.Loaded,
				_ => ModelRenderState.FailedToLoad
			};

			// We can dispose unless we wish to stay connected. Which I think is unecessary
			await CrashDocRegistry.DisposeOfDocumentAsync(Doc);
		}
		catch { }
	}

	private CrashDoc GetCrashDoc()
	{
		var doc = new CrashDoc();
		doc.Users.CurrentUser = new User(UserName);
		return doc;
	}

}
