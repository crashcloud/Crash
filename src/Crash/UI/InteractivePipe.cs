using System.Drawing;

using Crash.Common.App;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers;
using Crash.Handlers.Plugins;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.UI
{
	/// <summary>
	///     Interactive pipeline for crash geometry display
	/// </summary>
	internal sealed class InteractivePipe : ICrashInstance
	{
		private readonly Dictionary<string, IChangeDefinition> definitionRegistry;

		internal static InteractivePipe GetActive(CrashDoc crashDoc)
		{
			CrashInstances.TryGetInstance(crashDoc, out InteractivePipe activePipe);
			if (activePipe is null)
			{
				activePipe = new InteractivePipe(crashDoc);
				CrashInstances.TrySetInstance(crashDoc, activePipe);
			}
			return activePipe;
		}

		private readonly DisplayMaterial cachedMaterial = new(Color.Blue);

		// TODO : Does this ever get shrunk? It should do.
		// TODO : Don't draw things not in the view port
		private BoundingBox bbox;

		/// <summary>
		///     Empty constructor
		/// </summary>
		internal InteractivePipe(CrashDoc crashDoc)
		{
			bbox = new BoundingBox(-100, -100, -100, 100, 100, 100);
			CrashInstances.TrySetInstance(crashDoc, this);
			definitionRegistry = new Dictionary<string, IChangeDefinition>();
		}

		private bool enabled { get; set; }

		/// <summary>
		///     Pipeline enabled, disabling hides it
		/// </summary>
		internal bool Enabled
		{
			get => enabled;
			set
			{
				if (enabled == value)
				{
					return;
				}

				enabled = value;

				if (enabled)
				{
					DisplayPipeline.CalculateBoundingBox += CalculateBoundingBox;
					DisplayPipeline.PostDrawObjects += PostDrawObjects;
				}
				else
				{
					DisplayPipeline.CalculateBoundingBox -= CalculateBoundingBox;
					DisplayPipeline.PostDrawObjects -= PostDrawObjects;
				}
			}
		}

		/// <summary>
		///     Method to calculate the bounding box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CalculateBoundingBox(object? sender, CalculateBoundingBoxEventArgs e)
		{
			e.IncludeBoundingBox(bbox);
		}

		/// <summary>
		///     Post draw object events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PostDrawObjects(object? sender, DrawEventArgs e)
		{
			var rhinoDoc = e.RhinoDoc;
			if (rhinoDoc is null) return;

			var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
			if (!crashDoc.Tables.TryGet<TemporaryChangeTable>(out var tempTable)) return;

			if (crashDoc?.Users is null)
			{
				return;
			}

			var caches = tempTable.GetChanges().ToList();
			var orderedCaches = caches.OrderBy(c => c.Owner);
			foreach (var change in orderedCaches)
			{
				if (e.Display.InterruptDrawing())
				{
					return;
				}

				if (!definitionRegistry.TryGetValue(change.Type, out var definition))
				{
					continue;
				}

				if (!crashDoc.Users.Get(change.Owner).Visible)
				{
					continue;
				}

				UpdateCachedMaterial(change);

				definition.Draw(e, cachedMaterial, change);
				var box = definition.GetBoundingBox(change);
				UpdateBoundingBox(box);
			}

			if (crashDoc?.Cameras is null)
			{
				return;
			}

			var activeCameras = crashDoc.Cameras.GetActiveCameras();
			foreach (var activeCamera in activeCameras)
			{
				if (e.Display.InterruptDrawing())
				{
					return;
				}

				if (activeCamera.Key.Camera != CameraState.Visible)
				{
					continue;
				}

				// TODO : Get User properly?
				var cameraChange = CameraChange.CreateNew(activeCamera.Value, "");

				if (!definitionRegistry.TryGetValue(cameraChange.Type,
													out var definition))
				{
					continue;
				}

				UpdateCachedMaterial(activeCamera.Key);
				definition.Draw(e, cachedMaterial, cameraChange);
				var box = definition.GetBoundingBox(cameraChange);
				UpdateBoundingBox(box);
			}
		}

		private void UpdateCachedMaterial(User user)
		{
			if (cachedMaterial.Diffuse.Equals(user.Color))
			{
				return;
			}

			cachedMaterial.Diffuse = user.Color;
		}

		private void UpdateCachedMaterial(IChange change)
		{
			UpdateCachedMaterial(new User(change.Owner));
		}

		/// <summary>
		///     Updates the BoundingBox of the Pipeline
		/// </summary>
		private void UpdateBoundingBox(BoundingBox changeBox)
		{
			changeBox.Inflate(1.25);
			bbox.Union(changeBox);
		}

		internal void RegisterChangeDefinition(IChangeDefinition changeDefinition)
		{
			definitionRegistry.Add(changeDefinition.ChangeName, changeDefinition);
		}

		public void ClearChangeDefinitions()
		{
			definitionRegistry.Clear();
		}
	}
}
