namespace Crash.Handlers.Data;

[Flags]
public enum ModelRenderState
{
	None = 0,
	// Add = 1 << 0,
	// Sandbox = 1 << 1,
	// Debug = 1 << 2,

	Loading = 1 << 3,
	Loaded = 1 << 4,
	FailedToLoad = 1 << 5,

	RightClick = 1 << 6,
	Selected = 1 << 7,
	MouseOver = 1 << 8,
}
