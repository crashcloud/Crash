namespace Crash.UI;

[Flags]
internal enum ModelRenderState
{
	None = 0,
	Add = 1 << 0,
	Sandbox = 1 << 1,

	Loading = 1 << 2,
	Loaded = 1 << 3,
	FailedToLoad = 1 << 4,

	RightClick = 1 << 5,
	Selected = 1 << 6,
	MouseOver = 1 << 7,
}
