﻿using System.Drawing;

using Crash.Handlers.Changes;
using Crash.Handlers.Plugins.Geometry.Create;
using Crash.Handlers.Plugins.Geometry.Recieve;

using Rhino.Display;
using Rhino.Geometry;

using Point = Rhino.Geometry.Point;

namespace Crash.Handlers.Plugins.Geometry
{
	/// <summary>Defines the Geometry Change Type to handle default Rhino Geometry</summary>
	public sealed class GeometryChangeDefinition : IChangeDefinition
	{
		/// <summary>Default Constructor</summary>
		public GeometryChangeDefinition()
		{
			CreateActions = new List<IChangeCreateAction>
			                {
				                new GeometryCreateAction(),
				                new GeometryRemoveAction(),
				                new GeometryUpdateAction(),
				                new GeometryTransformAction(),
				                new GeometryLockAction(),
				                new GeometryUnlockAction()
			                };
			RecieveActions = new List<IChangeRecieveAction>
			                 {
				                 new GeometryAddRecieveAction(),
				                 new GeometryRemoveRecieveAction(),
				                 new GeometryUpdateRecieveAction(),
				                 new GeometryTransformRecieveAction(),
				                 new GeometryLockRecieveAction(),
				                 new GeometryUnlockRecieveAction()
			                 };
		}


		public string ChangeName => GeometryChange.ChangeType;


		public IEnumerable<IChangeCreateAction> CreateActions { get; }


		public IEnumerable<IChangeRecieveAction> RecieveActions { get; }


		public void Draw(DrawEventArgs drawArgs, DisplayMaterial material, IChange change)
		{
			if (change is not GeometryChange geomChange)
			{
				return;
			}

			var drawWireframe = !drawArgs.Display.DisplayPipelineAttributes.ShadingEnabled;
			var geom = geomChange.Geometry;
			if (geom is Curve cv)
			{
				drawArgs.Display.DrawCurve(cv, material.Diffuse, 3);
			}
			else if (geom is Brep brep)
			{
				// TODO : Spheres draw BADLY in wireframe
				drawArgs.Display.DrawBrepWires(brep, material.Diffuse, -1);
				if (!drawWireframe)
				{
					drawArgs.Display.DrawBrepShaded(brep, material);
				}
			}
			else if (geom is Mesh mesh)
			{
				drawArgs.Display.DrawMeshWires(mesh, material.Diffuse, 2);
				if (!drawWireframe)
				{
					drawArgs.Display.DrawMeshShaded(mesh, material);
				}
			}
			else if (geom is Extrusion ext)
			{
				drawArgs.Display.DrawExtrusionWires(ext, material.Diffuse);
				if (!drawWireframe)
				{
					drawArgs.Display.DrawBrepShaded(Brep.TryConvertBrep(ext), material);
				}
				// TODO : Cache
			}
			else if (geom is TextEntity te)
			{
				drawArgs.Display.DrawText(te, material.Diffuse);
			}
			else if (geom is TextDot td)
			{
				drawArgs.Display.DrawDot(td, Color.White, material.Diffuse, material.Diffuse);
			}
			else if (geom is Surface srf)
			{
				if (!drawWireframe)
				{
					drawArgs.Display.DrawBrepShaded(Brep.TryConvertBrep(srf), material);
				}

				if (srf.TryGetSphere(out var sphere, 0.1))
				{
					var latCircle = sphere.LatitudeDegrees(90);
					var longCircle = sphere.LongitudeDegrees(0);
					var longCircle2 = sphere.LongitudeDegrees(90);
					drawArgs.Display.DrawCircle(latCircle, material.Diffuse, 3);
					drawArgs.Display.DrawCircle(longCircle, material.Diffuse, 3);
					drawArgs.Display.DrawCircle(longCircle2, material.Diffuse, 3);
				}
				else
				{
					drawArgs.Display.DrawSurface(srf, material.Diffuse, 1);
				}

				// TODO : Cache
			}
			else if (geom is SubD subD)
			{
				if (!drawWireframe)
				{
					drawArgs.Display.DrawSubDShaded(subD, material);
				}

				drawArgs.Display.DrawSubDWires(subD, material.Diffuse, 1);
			}
			else if (geom is Point pnt)
			{
				drawArgs.Display.DrawPoint(pnt.Location, material.Diffuse);
			}
			else if (geom is AnnotationBase ab)
			{
				drawArgs.Display.DrawAnnotation(ab, material.Diffuse);
			}
		}


		public BoundingBox GetBoundingBox(IChange change)
		{
			if (change is not GeometryChange geomChange || geomChange.Geometry is null)
			{
				return BoundingBox.Unset;
			}


			return geomChange.Geometry.GetBoundingBox(false);
		}
	}
}
