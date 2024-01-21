using System.Drawing;

using Crash.Common.Document;
using Crash.Handlers;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.UI.ExceptionsAndErrors
{
	public class BadChangePipeline : IDisposable
	{
		private readonly List<Guid> _changes;

		private readonly CrashDoc _crashDoc;
		private readonly RhinoDoc _rhinoDoc;

		/// <summary>
		///     Empty constructor
		/// </summary>
		internal BadChangePipeline(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			_rhinoDoc = CrashDocRegistry.GetRelatedDocument(_crashDoc);
			_changes = new List<Guid>();
			Enabled = false;
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
			if (_crashDoc?.RealisedChangeTable is null)
			{
				return;
			}

			if (_crashDoc?.Users is null)
			{
				return;
			}

			foreach (var _changeId in _changes.ToHashSet())
			{
				if (!_crashDoc.RealisedChangeTable.TryGetRhinoId(_changeId, out var rhinoId))
				{
					continue;
				}

				var rhinoObject = _rhinoDoc.Objects.FindId(rhinoId);
				if (rhinoObject is null)
				{
					_changes.Remove(_changeId);
					continue;
				}

				var boundingBox = rhinoObject.Geometry.GetBoundingBox(Plane.WorldXY);
				boundingBox.Inflate(1.2);

				e.Display.DrawBox(boundingBox, Color.Red, 2);
				foreach (var corner in boundingBox.GetCorners())
				{
					e.Display.DrawDot(corner, "⚠️", Color.Red, Color.White);
				}
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

			foreach (var _changeId in _changes)
			{
				if (!_crashDoc.RealisedChangeTable.TryGetRhinoId(_changeId, out var rhinoId))
				{
					continue;
				}

				var rhinoObject = _rhinoDoc.Objects.FindId(rhinoId);
				if (rhinoObject is null)
				{
					continue;
				}

				var boundingBox = rhinoObject.Geometry.GetBoundingBox(Plane.WorldXY);
				boundingBox.Inflate(5);

				e.IncludeBoundingBox(boundingBox);
			}
		}

		internal void Push(IEnumerable<Guid> changeIds)
		{
			Enabled = true;
			_changes.AddRange(changeIds);
		}
	}
}
