﻿using Crash.Common.Document;
using Crash.Handlers;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>
	///     A wrapper for Rhino Commands to make Async awaiting easier and more centralised.
	///     https://discourse.mcneel.com/t/making-async-calls-from-runcommand/143160/7
	/// </summary>
	public abstract class AsyncCommand : Command
	{
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			// var commandTask = RunCommandAsync(doc, crashDoc, mode);

			try
			{
#pragma warning disable VSTHRD110 // Observe result of async calls
				RunCommandAsync(doc, crashDoc, mode);
#pragma warning restore VSTHRD110 // Observe result of async calls

				return Result.Success;
			}
			catch (Exception ex)
			{
				RhinoApp.WriteLine(ex.Message);
				return Result.Failure;
			}
		}

		protected abstract Task<Result> RunCommandAsync(RhinoDoc rhinoDoc, CrashDoc crashDoc, RunMode mode);
	}
}
