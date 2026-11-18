using ClickClub.Mobile.UI;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace ClickClub.Mobile;

public sealed class App : Application
{
    public App()
    {
        var webView=new BlazorWebView{HostPage="wwwroot/index.html",BackgroundColor=Color.FromArgb("#F7F8F4")};
        webView.RootComponents.Add(new RootComponent{Selector="#app",ComponentType=typeof(MobileRoot)});
        MainPage=new ContentPage{BackgroundColor=Color.FromArgb("#F7F8F4"),Content=webView};
    }
}
