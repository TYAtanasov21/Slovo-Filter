using System.Data;
using System.Diagnostics;
using Microsoft.Maui.Controls;
namespace Slovo_Filter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        MainPage = new NavigationPage(new LoginPage());
        
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        const int newWidth = 100;
        const int newHeight = 50;
        
        window.Width = newWidth;
        window.Height = newHeight;
        return window;
    }
    
    
    
}