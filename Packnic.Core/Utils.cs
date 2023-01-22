using CurseForge.APIClient;
using Packnic.Core.Model;

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

    public static bool ExactEqual<T>(this T[] array1, T[] array2)
    {
        if (array1.Length != array2.Length)
        {
            return false;
        }
        for (int i = 0; i < array1.Length; i++)
        {
            T a = array1[i];
            T b = array2[i];

            if ((a is null && b is not null)||(b is null && a is not null))
            {
                return false;
            }

            if (!a!.Equals(b))
            {
                return false;
            }
        }

        return true;
    }

    public static LocalFileNode? FindById(this List<LocalFileNode> list, Guid id)
    {
        return list.Find(node => node.Id.Equals(id));
    }

    public static LocalFileNode? FindByName(this List<LocalFileNode> list, string name)
    {
        return list.Find(node => node.Name == name);
    }

    public static void AddFile(this List<LocalFileNode> list, LocalFile file)
    {
        list.Add(new LocalFileNode(file));
    }
}