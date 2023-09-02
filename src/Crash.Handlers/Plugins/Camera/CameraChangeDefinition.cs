using Crash.Common.Changes;
using Crash.Handlers.Plugins.Camera.Create;
using Crash.Handlers.Plugins.Camera.Recieve;

using Rhino.Display;
using Rhino.Geometry;

namespace Crash.Handlers.Plugins.Camera
{
	/// <summary>Defines the Camera Change Type</summary>
	public sealed class CameraChangeDefinition : IChangeDefinition
	{
		private CameraGraphic Active;

		/// <summary>Constructs the Definition</summary>
		public CameraChangeDefinition()
		{
			CreateActions = new List<IChangeCreateAction> { new CameraCreateAction() };
			RecieveActions = new List<IChangeRecieveAction> { new CameraRecieveAction() };
		}

		public string ChangeName => CameraChange.ChangeType;

		public IEnumerable<IChangeCreateAction> CreateActions { get; }

		public IEnumerable<IChangeRecieveAction> RecieveActions { get; }

		public void Draw(DrawEventArgs drawArgs, DisplayMaterial material, IChange change)
		{
			if (change is not CameraChange cameraChange)
			{
				return;
			}

			Active = new CameraGraphic(cameraChange.Camera);
			Active.Draw(drawArgs, material);
		}

		public BoundingBox GetBoundingBox(IChange change)
		{
			var bounds = BoundingBox.Empty;
			if (Active.Lines is null || Active.Lines.Length == 0)
			{
				return bounds;
			}

			bounds = new BoundingBox(Active.Lines.Select(l => l.From));
			bounds.Inflate(1.25);

			return bounds;
		}

		/// <summary>The Camera Graphic for the Pipeline</summary>
		private readonly struct CameraGraphic
		{
			internal readonly Line[] Lines;
			internal readonly Plane Plane;
			private const double Length = 3000;
			private const double Width = 1920;
			private const double Height = 1080;
			private const double CrossHairLength = 80;

			private static readonly Interval WidthInterval = new(-Width / 2, Width / 2);
			private static readonly Interval HeightInterval = new(-Height / 2, Height / 2);

			// TODO : Scale correctly!
			internal CameraGraphic(Common.View.Camera camera)
			{
				var location = camera.Location.ToRhino();
				var target = camera.Target.ToRhino();
				var normal = target - location;

				var viewLine = new Line(location, normal, Length);

				var plane = GetCameraPlane(viewLine, normal);
				var rectangle = new Rectangle3d(plane, HeightInterval, WidthInterval);

				Lines = new Line[12];
				var rectangleSegs = rectangle.ToPolyline().GetSegments();
				Lines[0] = rectangleSegs[0];
				Lines[1] = rectangleSegs[1];
				Lines[2] = rectangleSegs[2];
				Lines[3] = rectangleSegs[3];
				Lines[4] = new Line(location, rectangle.PointAt(0));
				Lines[5] = new Line(location, rectangle.PointAt(1));
				Lines[6] = new Line(location, rectangle.PointAt(2));
				Lines[7] = new Line(location, rectangle.PointAt(3));

				var xVec = plane.XAxis;
				var yVec = plane.YAxis;
				var l1 = new Line(plane.Origin, xVec * CrossHairLength);
				var l2 = new Line(plane.Origin, xVec * -CrossHairLength);
				var l3 = new Line(plane.Origin, yVec * CrossHairLength);
				var l4 = new Line(plane.Origin, yVec * -CrossHairLength);

				Lines[8] = l1;
				Lines[9] = l2;
				Lines[10] = l3;
				Lines[11] = l4;
			}

			private Plane GetCameraPlane(Line viewLine, Vector3d normal)
			{
				var origin = viewLine.To;
				var cameraFrustrum = new Plane(origin, normal);
				var circ = new Circle(cameraFrustrum, 100);

				var xPoint = origin;
				xPoint.Transform(Transform.Translation(new Vector3d(0, 0, -100)));
				circ.ClosestParameter(xPoint, out var xParam);
				xPoint = circ.PointAt(xParam);

				var quarter = getUnParameterised(circ.Circumference, 0.25);
				var yParam = xParam - quarter;
				var yPoint = circ.PointAt(yParam);

				var plane = new Plane(origin, xPoint, yPoint);
				return plane;
			}

			private double getReparameterised(double length, double param)
			{
				return param / length;
			}

			private double getUnParameterised(double length, double param)
			{
				return length * param;
			}

			public void Draw(DrawEventArgs drawArgs, DisplayMaterial material)
			{
				// drawArgs.Display.DrawLines(Lines, PatternPen);
			}
		}
	}
}
