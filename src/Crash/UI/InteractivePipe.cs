using System.Drawing;

using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers;
using Crash.Handlers.Plugins;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.UI
{
	/// <summary>
	///     Interactive pipeline for crash geometry display
	/// </summary>
	// TODO : Make this static, and turn it into a template that just
	// grabs things from the CrashDoc
	// There's no need to recreate the same class again and again
	// and store so much geometry.
	internal sealed class InteractivePipe : IDisposable
	{
		private static readonly Dictionary<string, IChangeDefinition> definitionRegistry;


		internal static InteractivePipe Active;

		private readonly DisplayMaterial cachedMaterial = new(Color.Blue);

		// TODO : Does this ever get shrunk? It should do.
		// TODO : Don't draw things not in the view port
		private BoundingBox bbox;

		static InteractivePipe()
		{
			definitionRegistry = new Dictionary<string, IChangeDefinition>();
		}

		/// <summary>
		///     Empty constructor
		/// </summary>
		internal InteractivePipe()
		{
			bbox = new BoundingBox(-100, -100, -100, 100, 100, 100);
			Active = this;
		}

		private double scale => RhinoDoc.ActiveDoc is not null
			                        ? RhinoMath.UnitScale(UnitSystem.Meters, RhinoDoc.ActiveDoc.ModelUnitSystem)
			                        : 0;

		private int FAR_AWAY => (int)scale * 1_5000;
		private int VERY_FAR_AWAY => (int)scale * 7_5000;

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

		public void Dispose()
		{
		}

		/// <summary>
		///     Method to calculate the bounding box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CalculateBoundingBox(object sender, CalculateBoundingBoxEventArgs e)
		{
			e.IncludeBoundingBox(bbox);
		}

		/// <summary>
		///     Post draw object events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PostDrawObjects(object sender, DrawEventArgs e)
		{
			var rhinoDoc = RhinoDoc.ActiveDoc;
			if (rhinoDoc is null)
			{
				return;
			}

			var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
			if (crashDoc?.TemporaryChangeTable is null)
			{
				return;
			}

			if (crashDoc?.Users is null)
			{
				return;
			}

			var caches = crashDoc.TemporaryChangeTable.GetChanges().ToList();
			foreach (var change in caches)
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

		internal static void RegisterChangeDefinition(IChangeDefinition changeDefinition)
		{
			definitionRegistry.Add(changeDefinition.ChangeName, changeDefinition);
		}

		public static void ClearChangeDefinitions()
		{
			definitionRegistry.Clear();
		}
	}
}
