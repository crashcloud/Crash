using BidirectionalMap;

using Crash.Common.App;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Commands;

using Rhino;
using Rhino.DocObjects;
using Crash.Handlers.Data;
using Eto.Forms;

namespace Crash.Handlers
{
	/// <summary>
	///     Contains pairings of RhinoDocs and CrashDocs
	/// </summary>
	public static class CrashDocRegistry
	{
		private static readonly BiMap<RhinoDoc, CrashDoc> s_documentRelationship;

		static CrashDocRegistry()
		{
			s_documentRelationship = new BiMap<RhinoDoc, CrashDoc>();
		}

		/// <summary>
		///     Returns the Document Related to the given <see cref="RhinoDoc" />
		/// </summary>
		/// <param name="doc">The <see cref="RhinoDoc" /> to check for a related <see cref="CrashDoc" /></param>
		/// <returns>A <see cref="CrashDoc" /> if any found, null otherwise</returns>
		public static CrashDoc? GetRelatedDocument(RhinoDoc doc)
		{
			if (doc is null)
			{
				return null;
			}

			if (s_documentRelationship.Forward.ContainsKey(doc))
			{
				return s_documentRelationship.Forward[doc];
			}

			return null;
		}

		/// <summary>
		///     Returns the Document Related to the given <see cref="CrashDoc" />
		/// </summary>
		/// <param name="doc">The <see cref="CrashDoc" /> to check for a related <see cref="RhinoDoc" /></param>
		/// <returns>A <see cref="RhinoDoc" /> if any found, null otherwise</returns>
		public static RhinoDoc? GetRelatedDocument(CrashDoc doc)
		{
			foreach (var kvp in s_documentRelationship.Reverse)
			{
				if (kvp.Key.Equals(doc))
				{
					return kvp.Value;
				}
			}

			return null;
		}

		/// <summary>
		///     Returns all of the open <see cref="CrashDoc" />s. Closed Documents are discarded.
		/// </summary>
		public static IEnumerable<CrashDoc> GetOpenDocuments()
		{
			return s_documentRelationship.Forward.Values;
		}

		/// <summary>
		///     Creates a new <see cref="CrashDoc" /> and registers it to the Registry.
		/// </summary>
		/// <returns>The new <see cref="CrashDoc" /></returns>
		public static CrashDoc CreateAndRegisterDocument(RhinoDoc rhinoDoc)
		{
			if (s_documentRelationship.Forward.ContainsKey(rhinoDoc))
			{
				return s_documentRelationship.Forward[rhinoDoc];
			}

			var crashDoc = new CrashDoc();
			Register(crashDoc, rhinoDoc);
			DocumentRegistered?.Invoke(null, new CrashEventArgs(crashDoc));

			crashDoc.Queue.OnCompletedQueue += RedrawOnCompleted;
			crashDoc.Queue.OnItemProcessed += RedrawEverySoOften;
			crashDoc.LocalClient.OnStartInitialization += RegisterQueue;
			crashDoc.LocalClient.OnStartInitialization += RegisterInitialLoadingBar;

			return crashDoc;
		}

		private static void RegisterInitialLoadingBar(object? sender, CrashInitArgs e)
		{
			e.CrashDoc.LocalClient.OnStartInitialization -= RegisterInitialLoadingBar;
			RhinoApp.WriteLine($"Connected to Crash Server {e.CrashDoc.LocalClient.Url} successfully.");
			RhinoApp.WriteLine("Loading Changes from the server ...");

			double count = 0.0;
			double changeLoadAmount = 50.0;

			EventHandler<CrashEventArgs> initialLoadingBar = null!;
			initialLoadingBar = (_, itemArgs) =>
			{
				count++;
				double crashCount = e.ChangeCount;
				double percentage = changeLoadAmount + (count / crashCount * changeLoadAmount);

				LoadingUtils.SetState(itemArgs.CrashDoc, (LoadingUtils.LoadingState)(int)percentage, false);

				if (count < crashCount) return;
				e.CrashDoc.Queue.OnItemProcessed -= initialLoadingBar;
			};
			e.CrashDoc.Queue.OnItemProcessed += initialLoadingBar;
		}

		private static void RegisterQueue(object? sender, CrashInitArgs e)
		{
			e.CrashDoc.LocalClient.OnStartInitialization -= RegisterQueue;

			// Register Events
			EventHandler cycleQueueDelegate = null!;
			cycleQueueDelegate = (o, args) =>
								 {
									 e.CrashDoc.Queue.RunNextAction();
								 };
			RhinoApp.Idle += cycleQueueDelegate;

			// DeRegister Events
			EventHandler<CrashEventArgs> deRegisterQueueCycle = null!;
			deRegisterQueueCycle = (o, args) =>
								   {
									   DocumentDisposed -= deRegisterQueueCycle;
									   RhinoApp.Idle -= cycleQueueDelegate;
								   };

			DocumentDisposed += deRegisterQueueCycle;
		}

		private static void RedrawOnCompleted(object? sender, CrashEventArgs e)
		{
			var rhinoDoc = GetRelatedDocument(e.CrashDoc);
			rhinoDoc.Views.Redraw();
			ProessedCount = 0;
		}

		private static int ProessedCount { get; set; } = 0;

		private static void RedrawEverySoOften(object? sender, CrashEventArgs e)
		{
			ProessedCount++;
			if (ProessedCount >= 10)
			{
				RedrawOnCompleted(sender, e);
			}
		}

		private static void Register(CrashDoc crashDoc, RhinoDoc rhinoDoc)
		{
			s_documentRelationship.Add(rhinoDoc, crashDoc);
		}

		/// <summary>
		///     Disposes of the given <see cref="CrashDoc" /> and also disconnects any active connections.
		/// </summary>
		public static async Task DisposeOfDocumentAsync(CrashDoc crashDoc)
		{
			if (crashDoc is null) return;

			try
			{
				if (crashDoc.Queue is not null)
				{
					crashDoc.Queue.ForceCycleQueue();
					// DeRegister Events
					crashDoc.Queue.OnCompletedQueue -= RedrawOnCompleted;
				}

				if (crashDoc.LocalClient is not null)
				{
					await crashDoc.LocalClient.StopAsync();
				}

				// Remove Geometry
				var rhinoDoc = GetRelatedDocument(crashDoc);
				if (rhinoDoc is not null)
				{
					s_documentRelationship.Remove(rhinoDoc);

					var settings = new ObjectEnumeratorSettings
					{
						ActiveObjects = false,
						LockedObjects = true,
						HiddenObjects = true
					};
					var rhinoObjects = rhinoDoc.Objects.GetObjectList(settings);
					foreach (var rhinoObject in rhinoObjects)
					{
						rhinoDoc.Objects.Unlock(rhinoObject, true);
						rhinoDoc.Objects.Show(rhinoObject, true);
					}

					rhinoDoc.Objects.Clear();
				}

				// Dispose
				crashDoc?.Dispose();
				DocumentDisposed?.Invoke(null, new CrashEventArgs(crashDoc));
			}
			catch
			{

			}
		}

		/// <summary>
		///     Fired when a new <see cref="CrashDoc" /> is registered
		/// </summary>
		public static event EventHandler<CrashEventArgs> DocumentRegistered;

		/// <summary>
		///     Fired when a <see cref="CrashDoc" /> is disposed of
		/// </summary>
		public static event EventHandler<CrashEventArgs> DocumentDisposed;
	}
}
