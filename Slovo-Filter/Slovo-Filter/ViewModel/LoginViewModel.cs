using System.Windows.Input;
using Slovo_Filter_DAL.Models;
using System.Windows.Input;

namespace Slovo_Filter.ViewModel;

public class LoginViewModel : BaseViewModel
{
    private string _username;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }
    
    private string _password;

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    
    public ICommand LoginCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new Command(OnLoginClicked);
    }

    private async void OnLoginClicked()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            await App.Current.MainPage.DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }
        await App.Current.MainPage.Navigation.PushModalAsync(new LoginPage());
        
    }
    
    
    
}
