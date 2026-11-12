namespace BaroBackend.Domain.Metagame;

public static class Resources
{
    public const string CurrencyA = "currency_a";
    public const string CurrencyB = "currency_b";

    public static bool IsValidResourceId(string resourceId)
    {
        return resourceId is CurrencyA or CurrencyB;
    }
}