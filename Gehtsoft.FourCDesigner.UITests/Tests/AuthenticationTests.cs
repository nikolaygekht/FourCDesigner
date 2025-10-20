namespace Gehtsoft.FourCDesigner.UITests.Tests;

/// <summary>
/// UI tests for authentication flows.
/// </summary>
[Collection("UI Tests")]
public class AuthenticationTests : UiTestBase
{
    /// <summary>
    /// Scenario 1: Unauthenticated user accessing index.html should be redirected to login.html
    /// </summary>
    [Fact]
    public async Task IndexPage_WithoutSession_ShouldRedirectToLogin()
    {
        // Navigate to index.html
        await Page.GotoAsync($"{BaseUrl}/index.html");

        // Wait for navigation to complete
        await Page.WaitForLoadStateAsync();

        // Verify we were redirected to login page
        Page.Url.Should().Contain("/login.html", "unauthenticated users should be redirected to login");
    }
}
