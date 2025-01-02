using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Handlers;
using Crash.Handlers.Data;

using Eto.Forms;

using Rhino.UI;

[assembly: InternalsVisibleTo("Crash.UI.Tests")]

namespace Crash.UI.JoinView
{
	/// <summary>The Join View Model</summary>
	internal sealed class RecentViewModel : BaseViewModel
	{
		public Dialog<ISharedModel> Host { get; }
		private const string PREVIOUS_MODELS_KEY = "PREVIOUS_SHARED_MODELS";

		private static JsonSerializerOptions JsonOptions => new()
		{
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			IncludeFields = false
		};

		internal RecentViewModel(Control host)
		{
			var sharedModels = new List<ISharedModel>();
			sharedModels.Add(new AddModel());
			sharedModels.Add(new SandboxModel());

			if (SharedModelCache.TryLoadSharedModels(out var loadedSharedModels))
			{
				sharedModels.AddRange(loadedSharedModels);
			}

			SharedModels = new(sharedModels);
			Host = host.ParentWindow as Dialog<ISharedModel>;
		}

		internal ObservableCollection<ISharedModel> SharedModels { get; }

		internal bool ModelIsNew(ISharedModel model)
		{
			var alreadyExists = SharedModels.Select(sm => sm.ModelAddress?.ToUpperInvariant() ?? string.Empty)
											.Contains(model.ModelAddress.ToUpperInvariant());
			if (alreadyExists)
			{
				return false;
			}

			if (string.IsNullOrEmpty(model.ModelAddress))
			{
				return false;
			}

			return true;
		}

		internal bool AddSharedModel(ISharedModel model)
		{
			if (ModelIsNew(model))
			{
				model.State = ModelRenderState.FailedToLoad;
				SharedModels.Add(model);
				NewModel?.Invoke(this, model);
				GetConnectionStatus(model);
				return true;
			}

			return false;
		}

		private bool TryGetSelected(out ISharedModel selected)
		{
			selected = null;
			var models = SharedModels.Where(sm => sm.State.HasFlag(ModelRenderState.MouseOver))?.ToList();
			if (models is null) return false;
			if (models.Count != 1) return false;

			selected = models[0];
			return true;
		}

		public async Task GetConnectionStatus(ISharedModel model)
		{
			try
			{
				if (model is null or AddModel) return;
				if (string.IsNullOrEmpty(model.ModelAddress)) return;

				var doc = new CrashDoc();
				var userName = Guid.NewGuid().ToString();
				doc.Users.CurrentUser = new User(userName);
				var client = doc.LocalClient = new CrashClient(doc, new IClientOptions(true));
				client.RegisterConnection(userName, new Uri($"{model.ModelAddress}/Crash"));

				var state = model.State;
				state &= ~ModelRenderState.FailedToLoad;
				state &= ~ModelRenderState.Loaded;
				state |= ModelRenderState.Loading;
				model.State = state;
				Invalidate();

				var result = await client.StartLocalClientAsync();
				state |= result switch
				{
					null => ModelRenderState.Loaded,
					_ => ModelRenderState.FailedToLoad
				};
				state &= ~ModelRenderState.Loading;

				await CrashDocRegistry.DisposeOfDocumentAsync(doc);

				model.State = state;
			}
			catch { model.State = ModelRenderState.FailedToLoad; }

			Invalidate();
		}

		private void Invalidate()
		{
			Host.ParentWindow.Invalidate(true);
		}

		internal void JoinSelected()
		{
			try
			{
				var model = SharedModels.FirstOrDefault(sm => sm.State.HasFlag(ModelRenderState.MouseOver));
				if (model is SandboxModel)
					model = new SharedModel("https://sandbox.getoasis.app");
				Host.Close(model);
			}
			catch { }
			finally { Invalidate(); }
		}

		internal void ReloadAll()
		{
			foreach (var model in SharedModels)
			{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				GetConnectionStatus(model);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}

			Invalidate();
		}

		internal void ReloadSelected()
		{
			try
			{
				if (!TryGetSelected(out var sharedModel)) return;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				GetConnectionStatus(sharedModel);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
			catch { }
			finally { Invalidate(); }
		}

		internal void RemoveSelected()
		{
			if (!TryGetSelected(out var selected)) return;
			SharedModels.Remove(selected);
			RemoveModel?.Invoke(this, selected);
		}

		internal string VersionText
		{
			get
			{
				var assem = typeof(RecentViewModel).Assembly;
				var name = assem.GetName();
				return $"Version {name.Version} - wip";
			}
		}

		internal event EventHandler<ISharedModel> NewModel;
		internal event EventHandler<ISharedModel> RemoveModel;

	}
}
