using ClickClub.Mobile.Core;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace ClickClub.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder=MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<IDeviceStorage,DeviceStorage>();
        builder.Services.AddSingleton(sp=>new MobileService(new HttpClient(),sp.GetRequiredService<IDeviceStorage>(),Path.Combine(FileSystem.AppDataDirectory,"clickclub-cache.db3")));
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
        return builder.Build();
    }
}

public sealed class DeviceStorage : IDeviceStorage
{
    public string ApiUrl{get=>Preferences.Default.Get("api-url","http://10.0.2.2:5080/");set=>Preferences.Default.Set("api-url",value);}
    public Task<string?> GetSecretAsync(string key)=>SecureStorage.Default.GetAsync(key);
    public Task SetSecretAsync(string key,string value)=>SecureStorage.Default.SetAsync(key,value);
    public void RemoveSecret(string key)=>SecureStorage.Default.Remove(key);
}
