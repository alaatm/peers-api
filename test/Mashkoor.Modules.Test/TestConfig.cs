namespace Mashkoor.Modules.Test;

public static class TestConfig
{
    private static readonly Dictionary<string, string> _connStrings = [];
    public static readonly bool IsCi = Environment.GetEnvironmentVariable("CI") == "true";

    static TestConfig()
    {
        if (IsCi)
        {
            return;
        }

        if (_connStrings.Count == 0)
        {
            var path = Path.Join(
                Path.GetDirectoryName(typeof(TestConfig).Assembly.Location),
                "..",
                "..",
                "..",
                ".connstrs");

            foreach (var line in File.ReadAllLines(path))
            {
                var split = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                _connStrings[split[0].ToUpperInvariant()] = split[1];
            }
        }
    }

    public static string GetConnectionString(string local, string ci) => IsCi
        ? Environment.GetEnvironmentVariable(ci)
        : _connStrings[local.ToUpperInvariant()];
}
