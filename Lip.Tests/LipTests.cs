using Lip.Context;
using Moq;

namespace Lip.Tests;

public class LipTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("https://github-proxy.com", "https://go-module-proxy.com")]
    public void Constructor_ValidArguements_Passes(string githubProxy, string goModuleProxy)
    {
        // Arrange
        var runtimeConfig = new RuntimeConfig
        {
            GitHubProxy = githubProxy,
            GoModuleProxy = goModuleProxy
        };

        IContext context = new Mock<IContext>().Object;

        // Act
        Lip _ = new(runtimeConfig, context);

        // Assert
        // No need to assert.
    }
}
