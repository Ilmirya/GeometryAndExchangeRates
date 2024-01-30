namespace GeometryAndExchangeRates.Features.CurrencyRate;

public static class CurrencyRateInstaller
{
    public static IServiceCollection RegisterCurrencyRate(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => configuration.GetSection(nameof(CurrencyRateSettings)).Get<CurrencyRateSettings>());
        return services;
    }
}