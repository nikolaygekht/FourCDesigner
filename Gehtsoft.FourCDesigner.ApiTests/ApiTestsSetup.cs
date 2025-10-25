namespace Gehtsoft.FourCDesigner.ApiTests;

/// <summary>
/// One-time setup for all API tests.
/// Cleans up email queue before any tests run.
/// </summary>
public class ApiTestsSetup : IDisposable
{
    public ApiTestsSetup()
    {
        // Clean up email queue from previous test runs (once for all tests)
        CleanEmailQueue();
    }

    /// <summary>
    /// Cleans up the email queue folder to prevent emails from previous test runs from accumulating.
    /// </summary>
    private static void CleanEmailQueue()
    {
        var queueFolder = "./data/EmailQueue";
        var badEmailFolder = "./data/BadEmail";

        if (Directory.Exists(queueFolder))
        {
            foreach (var file in Directory.GetFiles(queueFolder, "*.json"))
            {
                try { File.Delete(file); } catch { /* Ignore deletion errors */ }
            }
        }

        if (Directory.Exists(badEmailFolder))
        {
            foreach (var file in Directory.GetFiles(badEmailFolder, "*.json"))
            {
                try { File.Delete(file); } catch { /* Ignore deletion errors */ }
            }
        }
    }

    public void Dispose()
    {
        // Cleanup after all tests complete (if needed)
    }
}

/// <summary>
/// Defines a test collection that uses the API tests setup.
/// All tests in this collection will share the one-time setup.
/// </summary>
[CollectionDefinition("ApiTests")]
public class ApiTestsCollection : ICollectionFixture<ApiTestsSetup>
{
}
