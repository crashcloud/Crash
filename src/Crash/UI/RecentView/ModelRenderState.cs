namespace Crash.UI;

[Flags]
internal enum ModelRenderState
{
	None = 0,
	Add = 1 << 0,
	Loading = 1 << 1,
	Loaded = 1 << 2,
	FailedToLoad = 1 << 3,
	RightClick = 1 << 4,
}
