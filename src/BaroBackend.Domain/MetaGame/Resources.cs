namespace BaroBackend.Domain.Metagame;

public static class Resources
{
    public const string TokenNamePrefix = "<token>";

    public static string HeroTokenId(string heroId)
    {
        return $"token:hero:{heroId}";
    }

    public static string HeroTokenName(string heroName)
    {
        return $"{TokenNamePrefix}{heroName}";
    }
}