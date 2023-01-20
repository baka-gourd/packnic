using CurseForge.APIClient;

namespace Packnic.Core;

public static class Utils
{
    public static HttpClient NormalClient { get; set; } = new();
    private static ApiClient? _cfClient;

    static ApiClient GetCfApiClient()
    {
        if (_cfClient is null)
        {
            throw new NullReferenceException("CfApiClient must be initialized.");
        }

        return _cfClient;
    }

    public static ApiClient CfClient
    {
        get => GetCfApiClient();
        private set => _cfClient = value;
    }

    public static void InitCfApi(string apiKey)
    {
        if (_cfClient is not null)
        {
            return;
        }
        CfClient = new ApiClient(apiKey, "contact@nptr.cc");
    }
}