using System.Net;
using System.Runtime.InteropServices;

using Crash.Properties;

using Eto.Drawing;
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
			Title = "Join Shared Model";
			Resizable = false;
			Minimizable = false;
			BackgroundColor = Color.FromArgb(0, 0, 0);
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

		internal string ChosenAddress { get; set; }

		private JoinViewModel Model { get; }
		protected static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

		private event EventHandler<EventArgs> AddNewModel;
		private event EventHandler<EventArgs> RemoveModel;
		private event EventHandler<EventArgs> RefreshModels;
		private event EventHandler<EventArgs> JoinModel;

		private void JoinWindow_Closed(object? sender, EventArgs e)
		{
			Model.SaveSharedModels(null, null);
		}

		private void SubscribeToEvents()
		{
			AddNewModel += (sender, args) =>
			               {
				               Model.AddSharedModel(Model.AddModel);
				               ActiveModels.Invalidate(true);
			               };

			JoinModel += (sender, args) =>
			             {
				             if (CurrentSelection is not SharedModel model)
				             {
					             return;
				             }

				             Close(model);
			             };

			RemoveModel += (sender, args) =>
			               {
				               if (CurrentSelection is not SharedModel model)
				               {
					               return;
				               }

				               Model.SharedModels.Remove(model);
				               ActiveModels.Invalidate(true);
			               };

			RefreshModels += (sender, args) =>
			                 {
				                 if (CurrentSelection is not SharedModel model)
				                 {
					                 return;
				                 }

				                 model.Connect();

				                 ActiveModels.Invalidate(true);
			                 };
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
				             TextColor = Palette.TextColour,
				             Enabled = true
			             };

			return button;
		}

		private Control CreateAddButtonContents(CellEventArgs args)
		{
			var modelContext = args.Item as SharedModel;

			var button = new Button
			             {
				             Text = "+",
				             ToolTip = "Add a new Shared Model",
				             Command = new Command(AddNewModel)
				                       {
					                       DataContext = modelContext, ToolTip = "Add Model to List"
				                       },
				             Enabled = false,
				             TextColor = Palette.TextColour
			             };

			modelContext.OnAddressChanged += (sender, args) =>
			                                 {
				                                 if (sender is not SharedModel model)
				                                 {
					                                 return;
				                                 }

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
