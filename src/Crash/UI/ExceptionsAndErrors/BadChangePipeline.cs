using System.Drawing;

using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Utils;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.UI.ExceptionsAndErrors
{
	public class BadChangePipeline : IDisposable
	{
		private readonly IEnumerable<Change> changes;

		private readonly CrashDoc crashDoc;

		/// <summary>
		///     Empty constructor
		/// </summary>
		internal BadChangePipeline(CrashChangeArgs args)
		{
			crashDoc = args.CrashDoc;
			changes = args.Changes;
			Enabled = true;
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
					DisplayPipeline.DrawOverlay += DrawOverlay;
					DisplayPipeline.CalculateBoundingBox += DisplayPipelineOnCalculateBoundingBox;
				}
				else
				{
					DisplayPipeline.DrawOverlay -= DrawOverlay;
					DisplayPipeline.CalculateBoundingBox -= DisplayPipelineOnCalculateBoundingBox;
				}
			}
		}

		public void Dispose()
		{
		}

		internal void DrawOverlay(object sender, DrawEventArgs e)
		{
			if (CrashDocRegistry.ActiveDoc?.TemporaryChangeTable is null)
			{
				return;
			}

			if (CrashDocRegistry.ActiveDoc?.Users is null)
			{
				return;
			}

			var count = 0;
			foreach (var change in changes)
			{
				if (!change.TryGetRhinoObject(crashDoc, out var rhinoObject))
				{
					continue;
				}

				count++;

				var boundingBox = rhinoObject.Geometry.GetBoundingBox(Plane.WorldXY);
				boundingBox.Inflate(1.2);

				e.Display.DrawBox(boundingBox, Color.Red, 2);
				foreach (var corner in boundingBox.GetCorners())
				{
					e.Display.DrawDot(corner, "⚠️", Color.Red, Color.White);
				}
			}

			if (count == 0)
			{
				Dispose();
			}
		}

		private void DisplayPipelineOnCalculateBoundingBox(object? sender, CalculateBoundingBoxEventArgs e)
		{
			if (CrashDocRegistry.ActiveDoc?.TemporaryChangeTable is null)
			{
				return;
			}

			if (CrashDocRegistry.ActiveDoc?.Users is null)
			{
				return;
			}

			foreach (var change in changes)
			{
				if (!change.TryGetRhinoObject(crashDoc, out var rhinoObject))
				{
					continue;
				}

				var boundingBox = rhinoObject.Geometry.GetBoundingBox(Plane.WorldXY);
				boundingBox.Inflate(5);

				e.IncludeBoundingBox(boundingBox);
			}
		}
	}
}
