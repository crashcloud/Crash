﻿using Crash.Handlers.InternalEvents;

using Rhino.Geometry;

namespace Crash.Handlers.Tests.Plugins.Geometry
{
	internal static class SharedUtils
	{
		internal static IEnumerable<CrashObject> SelectObjects
		{
			get
			{
				var geometryGen = new Func<GeometryBase>[]
				                  {
					                  NRhino.Random.Geometry.NBrep.Any, NRhino.Random.Geometry.NMesh.Any,
					                  NRhino.Random.Geometry.NLineCurve.Any
				                  };

				// TODO : Fix
				throw new NotImplementedException("Fix this!");
				for (var i = 0; i < geometryGen.Length; i++)
				{
					var geom = geometryGen[i]();
					for (var j = 0; j < 5; j++)
					{
						// yield return new CrashObject(Guid.NewGuid(), Guid.NewGuid(), geom);
					}
				}
			}
		}
	}
}
