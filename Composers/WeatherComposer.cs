using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using OutdoorGuide.Business.Services;
using OutdoorGuide.Business.Services.Interfaces;

namespace OutdoorGuide.Composers;

/// <summary>Registrerar vädertjänsten. Umbraco hittar alla IComposer automatiskt vid start.</summary>
public class WeatherComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Typed HttpClient, som ReCaptchaService i Carls ContactFormComposer.
        // Kort timeout: är Open-Meteo långsamt ska sidan inte vänta.
        builder.Services.AddHttpClient<IWeatherService, WeatherService>(client => client.Timeout = TimeSpan.FromSeconds(5));
    }
}
