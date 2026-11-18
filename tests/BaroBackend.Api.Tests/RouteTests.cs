namespace BaroBackend.Api.Tests;

[Collection("api")]
public sealed class RouteTests()
{
    [Fact]
    public void PlayerEndpoints_ResolveRoutesMethods_EqualGetRoutes()
    {
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetPlayerRoute, 
            PlayerEndpoints.ResolvePlayerRoute(PlayerEndpoints.PlayerIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetHeroesRoute,
            PlayerEndpoints.ResolvePlayerHeroesRoute(PlayerEndpoints.PlayerIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetHeroRoute,
            PlayerEndpoints.ResolvePlayerHeroRoute(PlayerEndpoints.PlayerIdParameter, PlayerEndpoints.HeroIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetUnlockHeroRoute,
            PlayerEndpoints.ResolvePlayerBuyUnlockHeroRoute(PlayerEndpoints.PlayerIdParameter, PlayerEndpoints.HeroIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetLevelUpHeroRoute,
            PlayerEndpoints.ResolvePlayerBuyLevelUpHeroRoute(PlayerEndpoints.PlayerIdParameter, PlayerEndpoints.HeroIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetHeroesRoute + PlayerEndpoints.DevUnlockOneSuffix,
            PlayerEndpoints.ResolvePlayerHeroesDevUnlockOneRoute(PlayerEndpoints.PlayerIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetHeroesRoute + PlayerEndpoints.DevUnlockAllSuffix,
            PlayerEndpoints.ResolvePlayerHeroesDevUnlockAllRoute(PlayerEndpoints.PlayerIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetBalancesRoute,
            PlayerEndpoints.ResolvePlayerBalancesRoute(PlayerEndpoints.PlayerIdParameter));
        Assert.Equal(
            PlayerEndpoints.BasePath + PlayerEndpoints.GetBalanceRoute,
            PlayerEndpoints.ResolvePlayerBalanceRoute(PlayerEndpoints.PlayerIdParameter, PlayerEndpoints.ResourceIdParameter));
    }
}
