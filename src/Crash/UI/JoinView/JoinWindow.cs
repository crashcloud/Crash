using System;

using Eto.Forms;
using Eto.Drawing;

using Rhino.UI;
using Crash.Properties;
using System.Net;
using System.Runtime.InteropServices;

namespace Crash.UI.JoinModel
{
	public partial class JoinWindow : Dialog<SharedModel>, IDisposable
	{

		internal string ChosenAddress { get; set; }

		private event EventHandler<EventArgs> AddNewModel;
		private event EventHandler<EventArgs> RemoveModel;
		private event EventHandler<EventArgs> RefreshModels;
		private event EventHandler<EventArgs> JoinModel;

		// internal JoinViewModel ViewModel { get; set; }

		internal static JoinWindow ActiveForm;

		private JoinViewModel Model { get; set; }
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
			Maximizable = false;

			SubscribeToEvents();
			Model = new JoinViewModel();
			InitializeComponent();
			ActiveForm = this;
			Closed += JoinWindow_Closed;
		}

		private void JoinWindow_Closed(object? sender, EventArgs e)
		{
			// SAVE THE SETTINSG
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
				if (ActiveModels.SelectedItem is not SharedModel model)
					return;

				this.Close(model);
			};

			RemoveModel += (sender, args) =>
			{
				if (ActiveModels.SelectedItem is not SharedModel model)
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
