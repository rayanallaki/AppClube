namespace ClickClub.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IDeviceStorage, DeviceStorage>();
        builder.Services.AddSingleton(services => new MobileService(
            services.GetRequiredService<HttpClient>(),
            services.GetRequiredService<IDeviceStorage>(),
            Path.Combine(FileSystem.AppDataDirectory, "clickclub-cache.db3")));

        return builder.Build();
    }
}

internal sealed class DeviceStorage : IDeviceStorage
{
    public string ApiUrl
    {
        get => Preferences.Default.Get("api-url", MobileService.DefaultApiUrl);
        set => Preferences.Default.Set("api-url", value);
    }

    public Task<string?> GetSecretAsync(string key) => SecureStorage.Default.GetAsync(key);

    public Task SetSecretAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);

    public void RemoveSecret(string key) => SecureStorage.Default.Remove(key);
}
