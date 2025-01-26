using System.ComponentModel;
using System.Windows.Input;
using Slovo_Filter_DAL.Repositories;

namespace Slovo_Filter.ViewModel;

public class LoginViewModel 
{
    private readonly UserRepository _userRepository;
    public ICommand NavigateToRegisterCommand { get; }

    public LoginViewModel()
    {
        _userRepository = new UserRepository();
        NavigateToRegisterCommand = new Command(OnNavigateToRegister);
    }
    private async void OnNavigateToRegister()
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
        }
            
    }

    public async Task<(bool, string)> LoginUserAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return(false, "Email and/or password are required");
        }

        try
        {
            var isAuthenticated = await _userRepository.LoginUserAsync(email, password);

            if (isAuthenticated)
            {
                return (true, "Login Successful");
            }
            else
            {
                return (false, "Login Failed");
            }
        }
        catch (Exception ex)
        {
            return (false, $"An error occured: {ex.Message}");
        }
    }
}
