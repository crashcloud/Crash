using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Crash.Handlers.Data;
using Crash.UI.RecentView;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;

using static Crash.UI.RecentView.CrashCommands;

namespace Crash.UI
{

	internal class OverflowLayout<TItem> : PixelLayout
	{
		public int ControlWidth { get; set; } = -1;
		public int ControlHeight { get; set; } = -1;
		public int Padding => RecentModelDialog.PreviewPadding;

		public ObservableCollection<TItem> DataStore { get; set; }
		private Func<TItem, Control> ControlFactory { get; }

		internal RightClickMenu RightClickMenu { get; }

		private CrashCommands CommandsInstance => (ParentWindow as RecentModelDialog)?.CommandsInstance!;

		public OverflowLayout(ObservableCollection<TItem> sharedModels, Func<TItem, Control> controlFactory)
		{
			ControlFactory = (i) =>
			{
				var control = controlFactory(i);
				SubscribeObjectToEvents(control);
				return control;
			};
			DataStore = sharedModels;

			RightClickMenu = new RightClickMenu(new()) { Visible = false };

			InitLayout();
			InitBindings();
		}

		// TODO : Use Parent to get the width
		private void InitLayout()
		{
			if (Controls is not IList<Control> controls) return;
			if (controls?.Count > 0) return;

			if (DataStore.Count > controls.Count)
			{
				for (int i = controls.Count; i < DataStore.Count; i++)
				{
					var control = ControlFactory(DataStore[i]);
					ControlWidth = control.Width;
					ControlHeight = control.Height;
					Add(control, 0, 0);
				}
			}
			else
			{
				controls.Clear();
				foreach (var item in DataStore)
				{
					var control = ControlFactory(item);
					ControlWidth = control.Width;
					ControlHeight = control.Height;
					Add(control, 0, 0);
				}
			}

			Add(RightClickMenu, 0, 0);

			RepositionLayout();
		}

		private void InitBindings()
		{
			Shown += (s, e) =>
			{
				RepositionLayout();

				ParentWindow.SizeChanged += (s, e) =>
				{
					Width = ParentWindow.Width;
					RepositionLayout();
					HideRightClick();
				};
				DataStore.CollectionChanged += (s, e) =>
				{
					// TODO : Implement
				};
			};
		}

		internal float RealWidth => ParentWindow?.Width ?? this.GetPreferredSize().Width;
		internal int HorizontalControlCount => (int)((RealWidth - Padding) / (ControlWidth + Padding));
		internal int VerticalControlCount => (int)Math.Ceiling(Controls.Count() / (float)HorizontalControlCount);

		public void RepositionLayout()
		{
			if (Controls is not IList<Control> controls) return;
			if (controls.Count == 0) return;

			for (int i = 0; i < HorizontalControlCount; i++)
			{
				for (int j = 0; j < VerticalControlCount; j++)
				{
					var index = i + j * HorizontalControlCount;
					if (index >= controls.Count) break;

					var control = controls[index];
					int x = (i * (ControlWidth + Padding)) + Padding;
					int y = (j * (ControlHeight + Padding)) + Padding;
					var point = new Point(x, y);
					SetLocation(control, point);
				}
			}

			// TODO : Sizing is flimsy
			if (Parent is not Scrollable scrollable) return;
			if (ParentWindow is null) return;
			scrollable.Height = ParentWindow.Height - 80;
			Height = (VerticalControlCount * (ControlHeight + Padding)) + Padding;

			Invalidate(true);
		}

		private void SubscribeObjectToEvents(Control control)
		{
			control.MouseDown += (s, e) =>
			{
				if (e.Buttons == MouseButtons.Alternate)
				{
					if (control is RecentModelControl recent)
					{
						var point = this.PointFromScreen(control.PointToScreen(e.Location));
						var model = recent.Model;

						var commands = new List<CrashCommand>() { CommandsInstance.Remove, CommandsInstance.Reload, };

						if (model is AddModel)
						{
							commands = new List<CrashCommand> { CommandsInstance.Add };
						}
						else if (model is SandboxModel)
						{
							commands.Remove(CommandsInstance.Remove);
						}

						if (model is not AddModel)
						{
							var join = CommandsInstance.Join;
							join.Enabled = false;
							if (recent.Model.State.HasFlag(ModelRenderState.Loaded))
							{
								join.Enabled = true;
							}
							commands.Insert(0, join);
						}

						ShowRightClick(point, commands);
					}
				}
				else
				{
					HideRightClick();
				}
			};

			control.KeyDown += (s, e) =>
			{
				if (e.Key == Keys.Escape)
					HideRightClick();
			};

			this.Shown += (s, e) =>
			{
				ParentWindow.KeyDown += (s, e) =>
				{
					if (e.Key == Keys.Escape)
						HideRightClick();
				};
			};
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Alternate)
			{
				ShowRightClick(e.Location, new() { CommandsInstance.ReloadAll });
			}
			else
			{
				HideRightClick();
			}

			base.OnMouseDown(e);
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Keys.Escape)
				HideRightClick();

			base.OnKeyDown(e);
		}

		private void HideRightClick()
		{
			RightClickMenu.Visible = false;
			// Remove(RightClickMenu);

			Invalidate(true);
		}

		private void ShowRightClick(PointF point, List<CrashCommand> commands)
		{
			RightClickMenu.Items.Clear();
			RightClickMenu.Visible = true;
			RightClickMenu.AddItems(commands);

			foreach (var child in Children.OfType<RecentModelControl>())
			{
				child.Model.State &= ~ModelRenderState.MouseOver;
				if (child.Bounds.Contains((Point)point))
				{
					child.Model.State |= ModelRenderState.MouseOver;
				}
			}

			var right = point.X + RightClickMenu.Width;
			var bottom = point.Y + RightClickMenu.Height;
			if (right > ParentWindow.Width)
				point.X -= RightClickMenu.Width;

			if (bottom > ParentWindow.Height - 80f)
				point.Y -= RightClickMenu.Height;

			Move(RightClickMenu, (int)point.X, (int)point.Y);
			Invalidate(true);
		}
	}
}
