﻿using Crash.Common.Document;

namespace Crash.Handlers.Plugins
{
	/// <summary>A wrapper for Crash Args</summary>
	public class CreateRecieveArgs : EventArgs
	{
		/// <summary>The Action</summary>
		public readonly ChangeAction Action;

		/// <summary>The EventArgs, often wrapped Rhino Args</summary>
		public readonly EventArgs Args;

		/// <summary>The current Crash Document</summary>
		public readonly CrashDoc Doc;

		/// <summary>Internal Constructor</summary>
		public CreateRecieveArgs(ChangeAction action, EventArgs args, CrashDoc doc)
		{
			Action = action;
			Args = args ?? throw new ArgumentNullException(nameof(args));
			Doc = doc ?? throw new ArgumentNullException(nameof(doc));
		}
	}
}
