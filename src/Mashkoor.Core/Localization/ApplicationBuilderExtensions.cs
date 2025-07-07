namespace Mashkoor.Core.Localization;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds request localization middleware to request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseLocalization(
        this IApplicationBuilder app)
    {
        // Uses localization middleware for setting headers
        // based on 'accept-language' header.

        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(Lang.SupportedLanguages[0])
            .AddSupportedCultures(Lang.SupportedLanguages)
            .AddSupportedUICultures(Lang.SupportedLanguages);
        localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
        app.UseRequestLocalization(localizationOptions);

        return app;
    }
}
