using Microsoft.AspNetCore.Components.WebView.Maui;

namespace ClickClub.Mobile;

public sealed class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var background = Color.FromArgb("#F7F8F4");
        var webView = new BlazorWebView
        {
            HostPage = "wwwroot/index.html",
            BackgroundColor = background
        };
        webView.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(MobileRoot)
        });

        return new Window(new ContentPage { BackgroundColor = background, Content = webView })
        {
            Title = "ClickClub",
            Width = 480,
            Height = 850,
            MinimumWidth = 360,
            MinimumHeight = 520
        };
    }
}
