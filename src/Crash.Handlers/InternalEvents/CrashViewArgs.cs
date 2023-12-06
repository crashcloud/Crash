using Crash.Common.Document;
using Crash.Geometry;

using Rhino.Display;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps Rhino View Event Args</summary>
	public sealed class CrashViewArgs : EventArgs
	{
		/// <summary>The Crash Doc of these Args</summary>
		public readonly CrashDoc Doc;

		/// <summary>The Camera Location of the Event</summary>
		public readonly CPoint Location;

		/// <summary>The Camera Target of the Event</summary>
		public readonly CPoint Target;

		/// <summary>Lazy Constructor</summary>
		internal CrashViewArgs(CrashDoc crashDoc, RhinoView view)
			: this(crashDoc, view.ActiveViewport.CameraLocation.ToCrash(),
			       view.ActiveViewport.CameraTarget.ToCrash())
		{
		}


		/// <summary>Constructor mainly for Tests</summary>
		internal CrashViewArgs(CrashDoc crashDoc, CPoint location, CPoint target)
		{
			Doc = crashDoc;
			Location = location;
			Target = target;
		}
	}
}
