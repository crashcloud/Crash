using Crash.Handlers.Data;

using Eto.Forms;

namespace Crash.UI;

internal class RecentModelControl : Drawable
{

	private RecentModelViewModel ViewModel => DataContext as RecentModelViewModel;

	public RecentModelControl(SharedModel model)
	{
		DataContext = new RecentModelViewModel(model);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Action<PaintEventArgs> render = ViewModel.State switch
		{
			ModelRenderState.Add => RenderAdd,
			ModelRenderState.Loading => RenderLoading,
			ModelRenderState.Loaded => RenderLoaded,
			ModelRenderState.FailedToLoad => RenderFailedToLoad,
			ModelRenderState.RightClick => RenderRightClick,
		};

		render(e);

		base.OnPaint(e);
	}

	private void RenderRightClick(PaintEventArgs args)
	{

	}

	private void RenderFailedToLoad(PaintEventArgs args)
	{

	}

	private void RenderLoaded(PaintEventArgs args)
	{

	}

	private void RenderLoading(PaintEventArgs args)
	{

	}

	private void RenderAdd(PaintEventArgs e)
	{

	}

}
