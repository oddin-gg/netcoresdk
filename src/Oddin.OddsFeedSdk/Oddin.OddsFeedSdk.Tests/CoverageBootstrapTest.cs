using Xunit;

namespace Oddin.OddsFeedSdk.Tests
{
    // CORE-3368: bootstrap test so `dotnet test` actually starts a test runner
    // and Coverlet can emit a Cobertura coverage report covering the SDK's
    // classes. Replace with real test cases as the SDK gains test coverage.
    public class CoverageBootstrapTest
    {
        [Fact]
        public void Bootstrap()
        {
        }
    }
}
