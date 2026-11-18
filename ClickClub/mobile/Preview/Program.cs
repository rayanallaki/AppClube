using ClickClub.Mobile.Core;
using ClickClub.Mobile.Preview;
var builder=WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped<IDeviceStorage,PreviewStorage>();
builder.Services.AddScoped(sp=>new MobileService(new HttpClient(),sp.GetRequiredService<IDeviceStorage>(),Path.Combine(Path.GetTempPath(),"clickclub-preview-"+Guid.NewGuid().ToString("N")+".db3")));
var app=builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<PreviewApp>().AddInteractiveServerRenderMode();
app.Run();

public sealed class PreviewStorage : IDeviceStorage
{
    readonly Dictionary<string,string> secrets=new();
    public string ApiUrl{get;set;}="http://localhost:5080/";
    public Task<string?> GetSecretAsync(string key)=>Task.FromResult(secrets.GetValueOrDefault(key));
    public Task SetSecretAsync(string key,string value){secrets[key]=value;return Task.CompletedTask;}
    public void RemoveSecret(string key)=>secrets.Remove(key);
}
