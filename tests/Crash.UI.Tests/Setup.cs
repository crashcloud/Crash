using NUnit.Framework;

[SetUpFixture]
public sealed class TestFixture
{
	[OneTimeSetUp]
	public void Init()
	{
		NUnitTestFixture testFixture = new NUnitTestFixture();
		testFixture.Init(new FixtureOptions());
	}

}
