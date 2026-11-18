using Android.App;
using Android.Runtime;
namespace ClickClub.Mobile;
[Application]
public class MainApplication(nint handle,JniHandleOwnership ownership) : MauiApplication(handle,ownership)
{
    protected override MauiApp CreateMauiApp()=>MauiProgram.CreateMauiApp();
}
