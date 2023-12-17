using System.Drawing;

using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Utils;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.UI.ExceptionsAndErrors
{
	public class BadChangePipeline : IDisposable
	{
		private readonly IEnumerable<Change> _changes;

		private readonly CrashDoc _crashDoc;

		/// <summary>
		///     Empty constructor
		/// </summary>
		internal BadChangePipeline(CrashChangeArgs args)
		{
			_crashDoc = args.CrashDoc;
			_changes = args.Changes;
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

		private void DrawOverlay(object sender, DrawEventArgs e)
		{
			if (_crashDoc?.TemporaryChangeTable is null)
			{
				return;
			}

			if (_crashDoc?.Users is null)
			{
				return;
			}

			var count = 0;
			foreach (var change in _changes)
			{
				if (!change.TryGetRhinoObject(_crashDoc, out var rhinoObject))
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
			if (_crashDoc?.TemporaryChangeTable is null)
			{
				return;
			}

			if (_crashDoc?.Users is null)
			{
				return;
			}

			foreach (var change in _changes)
			{
				if (!change.TryGetRhinoObject(_crashDoc, out var rhinoObject))
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
