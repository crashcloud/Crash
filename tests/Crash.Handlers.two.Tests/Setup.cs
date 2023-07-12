using System.Runtime.InteropServices.WindowsRuntime;

using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

[SetUpFixture]
public class TestFixture
{

	private static NUnitTestFixture _fxiture;

	public static NUnitTestFixture Fixture
	{
		get
		{
			if (_fxiture is null)
			{
				_fxiture = new NUnitTestFixture();
				_fxiture.Init(new FixtureOptions());
			}

			return _fxiture;
		}
	}

	[OneTimeSetUp]
	public void SetUpRhino()
	{
		if (Fixture is null)
		{
			NUnitTestFixture testFixture = new NUnitTestFixture();
			testFixture.Init(new FixtureOptions());
		}
	}

}
