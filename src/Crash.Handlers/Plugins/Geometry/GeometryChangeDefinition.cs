using System.Drawing;

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
				                new GeometryTransformAction(),
				                new GeometrySelectAction(),
				                new GeometryUnSelectAction()
			                };
			RecieveActions = new List<IChangeRecieveAction>
			                 {
				                 new GeometryAddRecieveAction(),
				                 new GeometryTemporaryAddRecieveAction(),
				                 new GeometryRemoveRecieveAction(),
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

			var drawWireframe = drawArgs.Display.DisplayPipelineAttributes.ShadingEnabled;
			var geom = geomChange.Geometry;
			if (geom is Curve cv)
			{
				drawArgs.Display.DrawCurve(cv, material.Diffuse, 2);
			}
			else if (geom is Brep brep)
			{
				if (drawWireframe)
				{
					drawArgs.Display.DrawBrepWires(brep, material.Diffuse, -1);
				}
				else
				{
					drawArgs.Display.DrawBrepShaded(brep, material);
				}
			}
			else if (geom is Mesh mesh)
			{
				if (drawWireframe)
				{
					drawArgs.Display.DrawMeshWires(mesh, material.Diffuse, 2);
				}
				else
				{
					drawArgs.Display.DrawMeshShaded(mesh, material);
				}
			}
			else if (geom is Extrusion ext)
			{
				if (drawWireframe)
				{
					drawArgs.Display.DrawExtrusionWires(ext, material.Diffuse);
				}
				else
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
				if (drawWireframe)
				{
					drawArgs.Display.DrawSurface(srf, material.Diffuse, 1);
				}
				else
				{
					drawArgs.Display.DrawBrepShaded(Brep.TryConvertBrep(srf), material);
				}
				// TODO : Cache
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
			if (change is not GeometryChange geomChange)
			{
				return BoundingBox.Unset;
			}

			return geomChange.Geometry.GetBoundingBox(false);
		}
	}
}
