using System.ComponentModel;
using System.Windows.Input;
using Slovo_Filter_BLL.Services;
using Slovo_Filter_DAL.Repositories;

namespace Slovo_Filter.ViewModel;


public class LoginViewModel 
{
    public User User { get; private set; }
    private readonly UserRepository _userRepository;
    public ICommand NavigateToRegisterCommand { get; }
    public ICommand NavigateToMain { get; set; }

    public LoginViewModel()
    {
        _userRepository = new UserRepository();
        NavigateToRegisterCommand = new Command(OnNavigateToRegister);
        NavigateToMain = new Command(OnNavigateToMain);
    }
    private async void OnNavigateToRegister()
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
        }
    }

    private async void OnNavigateToMain()
    {
        Console.WriteLine("OnNavigateToMain");
        Console.WriteLine(User.Email);
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new MainApp(User));
        }
    }
    

    public async Task<(bool, string)> LoginUserAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Email and/or password are required");
        }

        try
        {
            var isAuthenticated = await _userRepository.LoginUserAsync(email, password);

            if (isAuthenticated)
            {
                // Fetch the user's details from the database
                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user != null)
                {
                    User = user;  // Store the logged-in user in the ViewModel
                    return (true, "Login Successful");
                }
                else
                {
                    return (false, "User data not found");
                }
            }
            else
            {
                return (false, "Login Failed");
            }
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }
}
