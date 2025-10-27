namespace LolApi.Riot
{
    /// <summary>
    /// Provides a static, read‑only view of the Riot API key.
    /// </summary>
    public static class RiotApiKey
    {
        private static readonly Lazy<string> _lazy = new(() =>
    {
        // Grab the root service provider that we stored after building the host
        var provider = CuntyMcCuntface.ServiceProvider
                     ?? throw new InvalidOperationException(
                            "ServiceProvider is not set - the host hasn't been built yet.");

        // Resolve the options (this will throw if the key is missing, which is fine)
        var opts = provider.GetRequiredService<RiotOptions>();
        if (string.IsNullOrWhiteSpace(opts.ApiKey))
            throw new InvalidOperationException("Riot API key is empty.");

        return opts.ApiKey;
    });

    public static string Value => _lazy.Value;
    }
}