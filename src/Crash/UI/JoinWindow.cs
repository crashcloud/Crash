using System;

using Eto.Forms;
using Eto.Drawing;

using Rhino.UI;
using Crash.Properties;
using static Crash.UI.SharedModelViewModel;
using System.Net;
using System.Runtime.InteropServices;

namespace Crash.UI
{
	public partial class JoinWindow : Form, IDisposable
	{

		internal string ChosenAddress { get; set; }

		private event EventHandler<EventArgs> Clicked;
		private event EventHandler<EventArgs> AddNewModel;
		private event EventHandler<EventArgs> RemoveModel;
		private event EventHandler<EventArgs> RefreshModels;
		private event EventHandler<EventArgs> JoinModel;

		// internal JoinViewModel ViewModel { get; set; }

		internal static JoinWindow ActiveForm;

		private SharedModelViewModel Model { get; set; }
		protected static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

		internal JoinWindow()
		{
			Title = "Join Shared Model";
			Resizable = false;
			Minimizable = false;
			AutoSize = true;
			Icon = Icons.crashlogo.ToEto();
			Topmost = true;
			WindowState = WindowState.Normal;
			ShowActivated = true;
			Maximizable = false;

			Model = new SharedModelViewModel();
			InitializeComponent();
			ActiveForm = this;
			SubscribeToEvents();
		}

		private void SubscribeToEvents()
		{
			AddNewModel += (sender, args) =>
			{
				this.Model.AddSharedModel(this.Model.AddModel);
				ActiveModels.Invalidate(true);
			};

			JoinModel += (sender, args) =>
			{
				if (sender is not SharedModel model)
					return;

				ChosenAddress = model.ModelAddress;
			};

			RemoveModel += (sender, args) =>
			{
				if (sender is not SharedModel model)
					return;

				Model.SharedModels.Remove(model);
				ActiveModels.Invalidate(true);
			};

			RefreshModels += (sender, args) =>
			{
				foreach(var model in this.Model.SharedModels)
				{
					model.Connect();
				}

				ActiveModels.Invalidate(true);
			};

		}

		private Control CreateOpenButtonContents(CellEventArgs args)
		{
			var modelContext = args.Item as SharedModel;

			var button = new Button()
			{
				Text = "Join",
				ToolTip = "Join a Shared Model",
				Command = new Command(JoinModel)
				{
					DataContext = modelContext,
					ToolTip = "Join Shared Model",
				},
				Enabled = false,
				TextColor = Palette.TextColour
			};

			return button;
		}

		private Control CreateAddButtonContents(CellEventArgs args)
		{
			var modelContext = args.Item as SharedModel;

			var button = new Button()
			{
				Text = "+",
				ToolTip = "Add a new Shared Model",
				Command = new Command(AddNewModel)
							{
								DataContext = modelContext,
								ToolTip = "Add Model to List"
							},
				Enabled = false,
				TextColor = Palette.TextColour
			};

			modelContext.OnAddressChanged += (sender, args) =>
			{
				if (sender is not SharedModel model)
					return;

				if (URLIsValid(modelContext.ModelAddress))
				{
					button.Enabled = true;
				}
			};

			return button;
		}

		private bool URLIsValid(string modelAddress)
		{
			try
			{
				var uri = new Uri(modelAddress);
				return true;
			}
			catch
			{
				return IPAddress.TryParse(modelAddress, out _);
			}
		}

	}
}
