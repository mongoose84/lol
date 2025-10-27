namespace LolApi.Riot;


public sealed class RiotOptions
{
    public const string SectionName = "Riot";

    public string ApiKey { get; init; } = string.Empty;
}