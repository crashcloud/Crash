﻿using System.Runtime.InteropServices;

using Crash.Properties;

using Eto.Forms;

using Rhino.UI;

namespace Crash.UI.JoinModel
{
	public partial class JoinWindow : Dialog<SharedModel>, IDisposable
	{
		// internal JoinViewModel ViewModel { get; set; }

		internal static JoinWindow ActiveForm;

		internal JoinWindow()
		{
			InitStyles();

			Title = "Join Shared Model";
			Resizable = false;
			Minimizable = false;
			Icon = Icons.crashlogo.ToEto();
#if NET7_0
			this.UseRhinoStyle();
#endif
			WindowState = WindowState.Normal;
			Maximizable = false;

			SubscribeToEvents();
			Model = new JoinViewModel();
			InitializeComponent();
			ActiveForm = this;
			Closed += JoinWindow_Closed;
		}

		internal string ChosenAddress { get; set; }

		private JoinViewModel Model { get; }
		protected static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

		private event EventHandler<EventArgs> AddNewModel;
		private event EventHandler<EventArgs> RemoveModel;
		private event EventHandler<EventArgs> JoinModel;

		private void JoinWindow_Closed(object? sender, EventArgs e)
		{
			Model.SaveSharedModels(null, null);
		}

		private void SubscribeToEvents()
		{
			AddNewModel += (sender, args) =>
			               {
				               if (sender is not Command command)
				               {
					               return;
				               }

				               if (command.CommandParameter is not TextBox textbox)
				               {
					               return;
				               }

				               var url = textbox.Text;

				               Model.AddSharedModel(new SharedModel { ModelAddress = url });

				               textbox.Text = string.Empty;
				               textbox.Invalidate();
				               ActiveModels.Invalidate(true);
			               };

			JoinModel += (sender, args) =>
			             {
				             var model = GetModel(sender);
				             if (model is not null)
				             {
					             Close(model);
				             }
			             };

			RemoveModel += (sender, args) =>
			               {
				               var model = GetModel(sender);
				               if (model is not null)
				               {
					               Model.SharedModels.Remove(model);
					               ActiveModels.Invalidate(true);
				               }
			               };
		}

		private SharedModel GetModel(object sender)
		{
			if (sender is Command { DataContext: SharedModel model })
			{
				return model;
			}

			return CurrentSelection;
		}

		private Control CreateOpenButtonContents(CellEventArgs args)
		{
			var modelContext = args.Item as SharedModel;

			var button = new Button
			             {
				             Text = "Join",
				             ToolTip = "Join a Shared Model",
				             Command = new Command(JoinModel)
				                       {
					                       DataContext = modelContext, ToolTip = "Join Shared Model"
				                       },
				             Enabled = true
			             };

			return button;
		}
	}
}
