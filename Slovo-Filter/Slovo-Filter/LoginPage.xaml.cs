using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Slovo_Filter.ViewModel;
namespace Slovo_Filter;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;
    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        BindingContext = _viewModel;
    }

    private async void OnLoginButtonClicked(object sender, System.EventArgs e)
    {
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;
        
        var(isSignedIn, message) = await _viewModel.LoginUserAsync(email, password);
        
        if (isSignedIn)
        {
            Console.WriteLine("Successfully logged in");
            await DisplayAlert("Login Success", message, "OK");
            // Console.WriteLine(AppUser.UserId);
            await Navigation.PushAsync(new MainApp());
        }
        else
        {
            Console.WriteLine("Login Failed");
            await DisplayAlert("Login Failed", message, "OK");
        }
    }
    
}