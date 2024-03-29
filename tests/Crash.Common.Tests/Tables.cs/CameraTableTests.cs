﻿using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Common.View;
using Crash.Geometry;

namespace Crash.Common.Tables.Tests
{
	public class CameraTableTests
	{
		[Parallelizable]
		[TestCaseSource(typeof(CameraChanges), nameof(CameraChanges.TestCases))]
		public void TestAddCamera(CameraChange change)
		{
			// Arrange
			var crashDoc = new CrashDoc();
			crashDoc.Users.Add(change.Owner);
			var cameraTable = crashDoc.Cameras;
			Assert.That(cameraTable.TryAddCamera(change), Is.True);

			// Act
			var users = cameraTable.GetActiveCameras().Keys.ToArray();
			var cameras = cameraTable.GetActiveCameras().Values.ToArray();

			// Assert
			Assert.That(users.Length, Is.EqualTo(1));
			Assert.That(cameras.Length, Is.EqualTo(1));
		}

		[Parallelizable]
		[TestCaseSource(typeof(CameraChanges), nameof(CameraChanges.TestCases))]
		public void TestGetCamera(CameraChange change)
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var cameraTable = new CameraTable(crashDoc);
			cameraTable.TryAddCamera(change);

			// Act
			Assert.That(cameraTable.TryGetCamera(new User(change.Owner),
			                                       out var cameras), Is.True);

			// Assert
			Assert.That(cameras.Count, Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		[Parallelizable]
		public void TestAddMoreThanMaxCameras()
		{
			// Arrange
			var overMax = CameraTable.MAX_CAMERAS_IN_QUEUE + 5;
			var cameraTable = new CameraTable(new CrashDoc());

			var userName = "Jeff";
			var user = new User(userName);

			// Act
			for (var i = 0; i < overMax; i++)
			{
				var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
				var change = CameraChange.CreateNew(camera, userName);
				Assert.That(cameraTable.TryAddCamera(change), Is.True);
			}

			Assert.That(cameraTable, Is.Not.Empty);

			// Assert
			Assert.That(cameraTable.TryGetCamera(user, out var cameras), Is.True);
			Assert.That(cameras.Count, Is.EqualTo(CameraTable.MAX_CAMERAS_IN_QUEUE));
		}

		[Test]
		[Parallelizable]
		public void TestGetActiveCameras()
		{
			var userName = "Jeff";
			var crashDoc = new CrashDoc();
			crashDoc.Users.Add(userName);
			var cameraTable = crashDoc.Cameras;

			// Act
			for (var i = 0; i < 5; i++)
			{
				var camera = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
				var change = CameraChange.CreateNew(camera, userName);
				Assert.That(cameraTable.TryAddCamera(change), Is.True);
			}

			// Assert
			Assert.That(cameraTable, Is.Not.Empty);
			Assert.That(cameraTable.GetActiveCameras().Count, Is.EqualTo(1));
		}

		public sealed class CameraChanges
		{
			public static IEnumerable TestCases
			{
				get
				{
					var camera1 = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
					yield return CameraChange.CreateNew(camera1, "Jenny");

					var camera2 = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
					yield return CameraChange.CreateNew(camera2, "Jack");

					var camera3 = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
					yield return CameraChange.CreateNew(camera3, "Jeff");

					var camera4 = new Camera(CPoint.Origin, new CPoint(1, 2, 3));
					yield return CameraChange.CreateNew(camera4, "Jerry");
				}
			}
		}
	}
}
