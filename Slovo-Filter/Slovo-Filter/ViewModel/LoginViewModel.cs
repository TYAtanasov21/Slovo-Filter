using System.ComponentModel;
using System.Windows.Input;
using Slovo_Filter_BLL.Services;
using Slovo_Filter_DAL.Repositories;
using System.Threading.Tasks;

namespace Slovo_Filter.ViewModel;
public class LoginViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
        }
    }
    
    private string _email;
    public string Email
    {
        get => _email;
        set { _email = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Email))); }
    }

    private string _password;
    public string Password
    {
        get => _password;
        set { _password = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password))); }
    }

    public User User { get; private set; }
    private readonly UserRepository _userRepository;
    public ICommand NavigateToRegisterCommand { get; }
    public ICommand LoginCommand { get; }

    public LoginViewModel()
    {
        _userRepository = new UserRepository();
        NavigateToRegisterCommand = new Command(OnNavigateToRegister);
        LoginCommand = new Command(async () => await LoginAsync());
    }

    private async void OnNavigateToRegister()
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
        }
    }

    private async Task LoginAsync()
    {
        IsLoading = true; // Show loading indicator

        var email = Email; // Assuming you bind these properties
        var password = Password;

        var (isSuccess, message) = await LoginUserAsync(email, password);

        if (isSuccess)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new MainApp(User));
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Login Failed", message, "OK");
        }

        IsLoading = false; // Hide loading indicator
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
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user != null)
                {
                    User = user;
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
