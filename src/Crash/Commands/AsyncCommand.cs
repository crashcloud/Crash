using Crash.Common.Document;
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
		private bool Await { get; }

		internal AsyncCommand(bool await = false)
		{
			Await = await;
		}

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);

			try
			{
				if (Await)
				{
					Result result = Result.Failure;
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
					Eto.Forms.Application.Instance.AsyncInvoke(async () =>
					{
						try
						{
							result = await RunCommandAsync(doc, crashDoc, mode);
						}
						catch
						{

						}
					});
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
					return result;
				}
				else
				{
#pragma warning disable VSTHRD110 // Observe result of async calls
					RunCommandAsync(doc, crashDoc, mode);
#pragma warning restore VSTHRD110 // Observe result of async calls
				}

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
